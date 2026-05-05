using Common.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace RankCalculator;

public class RankProcessor
{
    private readonly IModel _channel;
    private readonly IDatabase _db;

    private record Message(string Id);

    public RankProcessor(IModel channel, IDatabase db)
    {
        _channel = channel;
        _db = db;
        _channel.ExchangeDeclare("events_exchange", ExchangeType.Fanout);
    }

    public async void Handle(object? sender, BasicDeliverEventArgs ea)
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var msg = JsonSerializer.Deserialize<Message>(json);

        if (msg == null) return;

        Console.WriteLine($"Processing {msg.Id}");

        TimeSpan interval = TimeSpan.FromSeconds(new Random().Next(3, 15));
        Console.WriteLine($"Waiting {interval}");
        await Task.Delay(interval);

        var text = _db.StringGet("TEXT-" + msg.Id);

        double rank = CalculateRank(text!);

        _db.StringSet("RANK-" + msg.Id, rank.ToString());

        PublishEvent(msg.Id, rank);
    }

    private void PublishEvent(string id, double rank)
    {
        var eventTypes = new EventTypes();
        var message = new EventMessage
        {
            Type = eventTypes.RankCalculated,
            Id = id,
            Rank = rank
        };

        var json = JsonSerializer.Serialize(message);

        _channel.BasicPublish(
            exchange: "events_exchange",
            routingKey: "",
            basicProperties: null,
            body: Encoding.UTF8.GetBytes(json)
        );
    }

    private double CalculateRank(string text)
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