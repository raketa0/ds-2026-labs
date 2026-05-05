using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Common.Events;
using Valuator.Hubs;

namespace Valuator.Services;

public class EventListener : BackgroundService
{
    private readonly IHubContext<SummaryHub> _hub;
    private readonly IConnection _connection;

    public EventListener(
        IHubContext<SummaryHub> hub,
        IConnection connection)
    {
        _hub = hub;
        _connection = connection;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();

        var queue = channel.QueueDeclare().QueueName;

        channel.QueueBind(queue, "events_exchange", "");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var msg = JsonSerializer.Deserialize<EventMessage>(json);

            string group = $"Text_{msg.Id}";

            await _hub.Clients.Group(group)
                .SendAsync("ReceiveUpdate", msg);
        };

        channel.BasicConsume(queue, true, consumer);

        return Task.CompletedTask;
    }
}