namespace Common.Events;

public class EventMessage
{
    public string Type { get; set; } = default!;
    public string Id { get; set; } = default!;

    public double? Rank { get; set; }
    public double? Similarity { get; set; }
}