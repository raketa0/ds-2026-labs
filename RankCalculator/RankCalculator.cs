using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

public class RankCalculator
{
    static void Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var rabbit =
            config.GetSection("RabbitMQ");

        var factory = new ConnectionFactory()
        {
            HostName = rabbit["Host"],
            UserName = rabbit["Username"],
            Password = rabbit["Password"]
        };

        using var connection =
            factory.CreateConnection();

        using var channel =
            connection.CreateModel();

        channel.QueueDeclare(
            "rank_queue",
            false,
            false,
            false
        );

        using var redis =
            ConnectionMultiplexer.Connect(
                config.GetConnectionString("Redis")
            );

        var db = redis.GetDatabase();

        var processor =
            new RankProcessor(channel, db);

        var consumer =
            new EventingBasicConsumer(channel);

        consumer.Received += processor.Handle;

        channel.BasicConsume(
            "rank_queue",
            true,
            consumer
        );

        Console.WriteLine(
            "RankCalculator started..."
        );

        Console.ReadLine();
    }
}