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

    private readonly ConnectionMultiplexer _mainRedis;

    private readonly Dictionary<string, ConnectionMultiplexer> _shards;

    private record Message(string Id);

    public RankProcessor(IModel channel)
    {
        _channel = channel;

        _channel.ExchangeDeclare("events_exchange", ExchangeType.Fanout);

        _mainRedis = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable("DB_MAIN")
            ?? "localhost:6000"
        );

        _shards = new Dictionary<string, ConnectionMultiplexer>
        {
            {
                "RU",
                ConnectionMultiplexer.Connect(
                    Environment.GetEnvironmentVariable("DB_RU")
                    ?? "localhost:6001"
                )
            },

            {
                "EU",
                ConnectionMultiplexer.Connect(
                    Environment.GetEnvironmentVariable("DB_EU")
                    ?? "localhost:6002"
                )
            },

            {
                "ASIA",
                ConnectionMultiplexer.Connect(
                    Environment.GetEnvironmentVariable("DB_ASIA")
                    ?? "localhost:6003"
                )
            }
        };
    }

    public void Handle(object? sender, BasicDeliverEventArgs ea)
    {
        var msg = JsonSerializer.Deserialize<Message>(
            Encoding.UTF8.GetString(ea.Body.ToArray())
        );


        var mainDb = _mainRedis.GetDatabase();

        string shard = mainDb.StringGet($"TEXT-SHARD-{msg.Id}");

        Console.WriteLine($"LOOKUP: {msg.Id}, {shard}");

        var db = _shards[shard].GetDatabase();

        var text = db.StringGet("TEXT-" + msg.Id);

        double rank = CalculateRank(text!);

        db.StringSet("RANK-" + msg.Id, rank.ToString());

        PublishEvent(msg.Id, shard, rank);
    }

    private void PublishEvent(string id, string shard, double rank)
    {
        var evt = new EventMessage
        {
            Type = new EventTypes().RankCalculated,
            Id = id,
            Shard = shard,
            Rank = rank
        };

        _channel.BasicPublish(
            "events_exchange",
            "",
            null,
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt))
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
                (c == 'ё') ||
                (c == 'Ё')
            ));

        return (double)nonAlphabetic / total;
    }
}