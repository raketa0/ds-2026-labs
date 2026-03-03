using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        // TODO: (pa1) проинициализировать свойства Rank и Similarity значениями из БД (Redis)
        var db = _redis.GetDatabase();

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
