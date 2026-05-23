using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Common.Events;

namespace Valuator.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConnection _rabbitConnection;

    public IndexModel(
        ILogger<IndexModel> logger,
        IConnectionMultiplexer redis,
        IConnection rabbitConnection)
    {
        _logger = logger;
        _redis = redis;
        _rabbitConnection = rabbitConnection;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return RedirectToPage();
        }

        string id = Guid.NewGuid().ToString();

        var db = _redis.GetDatabase();

        db.StringSet(
            "AUTHOR-" + id,
            User.Identity!.Name
        );

        string textKey = "TEXT-" + id;

        string similarityKey = "SIMILARITY-" + id;

        double similarity = CalculateSimilarity(text, db);

        db.StringSet(
            similarityKey,
            similarity.ToString()
        );

        db.StringSet(textKey, text);

        PublishRankRequest(id);

        PublishSimilarityEvent(id, similarity);

        return Redirect($"summary?id={id}");
    }

    private void PublishRankRequest(string id)
    {
        using var channel = _rabbitConnection.CreateModel();

        channel.QueueDeclare(
            "rank_queue",
            false,
            false,
            false
        );

        var message = JsonSerializer.Serialize(
            new { Id = id });

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            "",
            "rank_queue",
            null,
            body
        );
    }

    private void PublishSimilarityEvent(
        string id,
        double similarity)
    {
        using var channel = _rabbitConnection.CreateModel();

        channel.ExchangeDeclare(
            "events_exchange",
            ExchangeType.Fanout
        );

        var eventType = new EventTypes();

        var message = new EventMessage
        {
            Type = eventType.SimilarityCalculated,
            Id = id,
            Similarity = similarity
        };

        var json = JsonSerializer.Serialize(message);

        channel.BasicPublish(
            exchange: "events_exchange",
            routingKey: "",
            basicProperties: null,
            body: Encoding.UTF8.GetBytes(json)
        );
    }

    private double CalculateSimilarity(
        string text,
        IDatabase db)
    {
        var server = _redis.GetServer(
            _redis.GetEndPoints().First()
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