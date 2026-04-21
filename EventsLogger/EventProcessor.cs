using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class EventProcessor
{
    public record RankCalculatedEvent(string Id, double Rank);
    public record SimilarityCalculatedEvent(string Id, double Similarity);

    public void HandleRank(object? sender, BasicDeliverEventArgs ea)
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var evt = JsonSerializer.Deserialize<RankCalculatedEvent>(json);

        if (evt == null)
        {
            return;
        }

        Console.WriteLine($"RankCalculated: Id={evt.Id}, Rank={evt.Rank}");
    }

    public void HandleSimilarity(object? sender, BasicDeliverEventArgs ea)
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var evt = JsonSerializer.Deserialize<SimilarityCalculatedEvent>(json);

        if (evt == null)
        {
            return;
        }

        Console.WriteLine($"SimilarityCalculated: Id={evt.Id}, Similarity={evt.Similarity}");
    }
}