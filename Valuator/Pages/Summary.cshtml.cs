using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;

    private readonly IConnectionMultiplexer _mainRedis;

    private readonly Dictionary<string, IConnectionMultiplexer> _shards;

    public SummaryModel(
        ILogger<SummaryModel> logger,
        IConnectionMultiplexer mainRedis,
        Dictionary<string, IConnectionMultiplexer> shards)
    {
        _logger = logger;
        _mainRedis = mainRedis;
        _shards = shards;
    }

    public double Rank { get; set; }

    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        var mainDb = _mainRedis.GetDatabase();

        string shard = mainDb.StringGet($"TEXT-SHARD-{id}");

        Console.WriteLine($"LOOKUP: {id}, {shard}");

        var db = _shards[shard].GetDatabase();

        string rankKey = "RANK-" + id;

        string similarityKey = "SIMILARITY-" + id;

        RedisValue rankValue = db.StringGet(rankKey);

        RedisValue similarityValue = db.StringGet(similarityKey);

        Rank = 0;

        Similarity = 0;

        if (!rankValue.IsNull)
        {
            double.TryParse(rankValue.ToString(), out double r);

            Rank = r;
        }

        if (!similarityValue.IsNull)
        {
            double.TryParse(similarityValue.ToString(), out double s);

            Similarity = s;
        }
    }
}