using Common.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class EventsLogger
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: "events_exchange",
            type: ExchangeType.Fanout
        );

        var queueName = channel.QueueDeclare().QueueName;

        channel.QueueBind(
            queue: queueName,
            exchange: "events_exchange",
            routingKey: ""
        );


        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var msg = JsonSerializer.Deserialize<EventMessage>(json);

            if (msg.Type == "RankCalculated")
            {
                Console.WriteLine($"RankCalculated | Id={msg.Id} | Rank={msg.Rank} | Shard={msg.Shard}");
            }
            else if (msg.Type == "SimilarityCalculated")
            {
                Console.WriteLine($"SimilarityCalculated | Id={msg.Id} | Similarity={msg.Similarity} | Shard={msg.Shard}");
            }
        };

        channel.BasicConsume(queueName, true, consumer);

        Console.WriteLine("EventsLogger started...");
        Console.ReadLine();
    }
}