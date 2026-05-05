using Microsoft.AspNetCore.SignalR;

namespace Valuator.Hubs;

public class SummaryHub : Hub
{
    private readonly ILogger<SummaryHub> _logger;

    public SummaryHub(ILogger<SummaryHub> logger)
    {
        _logger = logger;
    }

    public async Task SubscribeToRankUpdate(string textId)
    {
        string group = $"Text_{textId}";

        await Groups.AddToGroupAsync(Context.ConnectionId, group);

        _logger.LogInformation(
            "Client {ConnectionId} subscribed to {Group}",
            Context.ConnectionId,
            group
        );
    }
}