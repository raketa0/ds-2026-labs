using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;


namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            return RedirectToPage();
        }

        string id = Guid.NewGuid().ToString();
        var db = _redis.GetDatabase();

        string textKey = "TEXT-" + id;
        // TODO: (pa1) сохранить в БД (Redis) text по ключу textKey

        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(text, db);
        db.StringSet(similarityKey, similarity.ToString());
        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey

        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var message = JsonSerializer.Serialize(new { Id = id });
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish("", "rank_queue", null, body);



        db.StringSet(textKey, text);
        return Redirect($"summary?id={id}");
    }

    private double CalculateSimilarity(string text, IDatabase db)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: "TEXT-*");

        foreach (var key in keys)
        {
            var existingText = db.StringGet(key);
            if (existingText == text)
                return 1;
        }

        return 0;
    }
}
