using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace RankCalculator;

internal class Program
{
    private static IConnection? _connection;
    private static IModel? _channel;
    private static IDatabase? _db;
    private record Message(string Id);

    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare("rank_queue", false, false, false);

        var redis = ConnectionMultiplexer.Connect("localhost");
        _db = redis.GetDatabase();

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += OnMessageReceived!;

        _channel.BasicConsume("rank_queue", true, consumer);

        Console.WriteLine("RankCalculator started...");
        Console.ReadLine();
    }

    private static void OnMessageReceived(object model, BasicDeliverEventArgs ea)
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var msg = JsonSerializer.Deserialize<Message>(json);

        if (msg == null)
        { 
            return;
        }

        Console.WriteLine($"Processing {msg.Id}");

        string? text = _db.StringGet("TEXT-" + msg.Id);

        double rank = CalculateRank(text);

        _db.StringSet("RANK-" + msg.Id, rank.ToString());

        Console.WriteLine($"Done {msg.Id}, rank={rank}");
    }

    private static double CalculateRank(string text)
    {
        int total = text.Length;

        int nonAlphabetic = text.Count(c =>
            !(
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= 'а' && c <= 'я') ||
                (c >= 'А' && c <= 'Я') ||
                (c == 'ё') || (c == 'Ё')
            ));

        return (double)nonAlphabetic / total;
    }


}