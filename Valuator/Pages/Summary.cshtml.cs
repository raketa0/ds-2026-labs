using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

[Authorize]
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    public SummaryModel(
        ILogger<SummaryModel> logger,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public double Rank { get; set; }

    public double Similarity { get; set; }

    public IActionResult OnGet(string id)
    {
        var db = _redis.GetDatabase();

        string authorKey = "AUTHOR-" + id;

        var author = db.StringGet(authorKey);

        if (author != User.Identity!.Name)
        {
            return Forbid();

        }

        string rankKey = "RANK-" + id;

        string similarityKey = "SIMILARITY-" + id;

        RedisValue rankValue =
            db.StringGet(rankKey);

        RedisValue similarityValue =
            db.StringGet(similarityKey);

        Rank = 0;
        Similarity = 0;

        if (!rankValue.IsNull)
        {
            double.TryParse(
                rankValue.ToString(),
                out double r);

            Rank = r;
        }

        if (!similarityValue.IsNull)
        {
            double.TryParse(
                similarityValue.ToString(),
                out double s);

            Similarity = s;
        }
        return Page();

    }
}