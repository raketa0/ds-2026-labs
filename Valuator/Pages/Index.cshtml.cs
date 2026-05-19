using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Common.Events;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    private readonly IConnectionMultiplexer _mainRedis;

    private readonly Dictionary<string, IConnectionMultiplexer> _shards;

    private readonly IConnection _rabbitConnection;

    public IndexModel(
        ILogger<IndexModel> logger,
        IConnectionMultiplexer mainRedis,
        Dictionary<string, IConnectionMultiplexer> shards,
        IConnection rabbitConnection)
    {
        _logger = logger;
        _mainRedis = mainRedis;
        _shards = shards;
        _rabbitConnection = rabbitConnection;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text, string country)
    {
        _logger.LogDebug(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            return RedirectToPage();
        }

        string shard = country switch
        {
            "Russia" => "RU",
            "France" => "EU",
            "Germany" => "EU",
            "UAE" => "ASIA",
            "India" => "ASIA",
            _ => throw new NotImplementedException()
        };

        string id = Guid.NewGuid().ToString();

        var mainDb = _mainRedis.GetDatabase();

        mainDb.StringSet($"TEXT-SHARD-{id}", shard);

        var db = _shards[shard].GetDatabase();

        string textKey = "TEXT-" + id;

        string similarityKey = "SIMILARITY-" + id;

        double similarity = CalculateSimilarity(text, db);

        db.StringSet(similarityKey, similarity.ToString());

        db.StringSet(textKey, text);

        PublishRankRequest(id);

        PublishSimilarityEvent(id, similarity, shard);

        return Redirect($"summary?id={id}");
    }

    private void PublishRankRequest(string id)
    {
        using var channel = _rabbitConnection.CreateModel();

        channel.QueueDeclare("rank_queue", false, false, false);

        var message = JsonSerializer.Serialize(new { Id = id });

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish("", "rank_queue", null, body);
    }

    private void PublishSimilarityEvent(string id, double similarity, string shard)
    {
        using var channel = _rabbitConnection.CreateModel();
        channel.ExchangeDeclare("events_exchange", ExchangeType.Fanout);

        var evt = new EventMessage
        {
            Type = new EventTypes().SimilarityCalculated,
            Id = id,
            Shard = shard,
            Similarity = similarity
        };

        channel.BasicPublish(
            "events_exchange",
            "",
            null,
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt))
        );
    }

    private double CalculateSimilarity(string text, IDatabase db)
    {
        var server = db.Multiplexer.GetServer(
            db.Multiplexer.GetEndPoints().First()
        );

        var keys = server.Keys(pattern: "TEXT-*");

        foreach (var key in keys)
        {
            var existingText = db.StringGet(key);

            if (existingText == text)
            {
                return 1;
            }
        }

        return 0;
    }
}