using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

public class RankCalculator
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare("rank_queue", false, false, false);

        var processor = new RankProcessor(channel);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += processor.Handle;

        channel.BasicConsume("rank_queue", true, consumer);

        Console.WriteLine("RankCalculator started...");
        Console.ReadLine();
    }
}