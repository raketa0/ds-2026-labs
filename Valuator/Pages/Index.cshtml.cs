using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

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

        string rankKey = "RANK-" + id;
        double rank = CalculateRank(text);
        db.StringSet(rankKey, rank.ToString());
        // TODO: (pa1) посчитать rank и сохранить в БД (Redis) по ключу rankKey

        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(text, db);
        db.StringSet(similarityKey, similarity.ToString());
        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey


        db.StringSet(textKey, text);
        return Redirect($"summary?id={id}");
    }

    private double CalculateRank(string text)
    {
        int total = text.Length;

        int nonAlphabetic = text.Count(c =>
            !(
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= 'а' && c <= 'я') ||
                (c >= 'А' && c <= 'Я')
             ));

        return (double)nonAlphabetic / total;
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
