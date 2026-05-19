using RabbitMQ.Client;
using StackExchange.Redis;

namespace Valuator;

public class Program
{
    //cоответсвие объяснить
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            string mainConnection =
                Environment.GetEnvironmentVariable("DB_MAIN")
                ?? "localhost:6000";

            return ConnectionMultiplexer.Connect(mainConnection);
        });

        builder.Services.AddSingleton<Dictionary<string, IConnectionMultiplexer>>(sp =>
        {
            return new Dictionary<string, IConnectionMultiplexer>
            {
                {
                    "RU",
                    ConnectionMultiplexer.Connect(
                        Environment.GetEnvironmentVariable("DB_RU")
                        ?? "localhost:6001"
                    )
                },

                {
                    "EU",
                    ConnectionMultiplexer.Connect(
                        Environment.GetEnvironmentVariable("DB_EU")
                        ?? "localhost:6002"
                    )
                },

                {
                    "ASIA",
                    ConnectionMultiplexer.Connect(
                        Environment.GetEnvironmentVariable("DB_ASIA")
                        ?? "localhost:6003"
                    )
                }
            };
        });

        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            return factory.CreateConnection();
        });

        builder.Services.AddRazorPages();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}