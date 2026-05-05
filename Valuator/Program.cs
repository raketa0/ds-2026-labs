using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Hubs;
using Valuator.Services;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var redis = ConnectionMultiplexer.Connect("localhost");

        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            return factory.CreateConnection();
        });

        builder.Services.AddRazorPages();

        builder.Services.AddSignalR()
            .AddStackExchangeRedis("localhost");

        builder.Services.AddHostedService<EventListener>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.MapHub<SummaryHub>("/summaryHub");

        app.Run();
    }
}