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

        var processor = new EventProcessor();

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            using var doc = JsonDocument.Parse(json);

            var type = doc.RootElement.GetProperty("Type").GetString();
            var id = doc.RootElement.GetProperty("Id").GetString();

            switch (type)
            {
                case "RankCalculated":
                    var rank = doc.RootElement.GetProperty("Rank").GetDouble();
                    Console.WriteLine($"RankCalculated | Id={id} | Rank={rank}");
                    break;

                case "SimilarityCalculated":
                    var sim = doc.RootElement.GetProperty("Similarity").GetDouble();
                    Console.WriteLine($"SimilarityCalculated | Id={id} | Similarity={sim}");
                    break;
            }
        };

        channel.BasicConsume(
            queue: queueName,
            autoAck: true,
            consumer: consumer
        );


        Console.WriteLine("EventsLogger started...");
        Console.ReadLine();
    }
}