using Microsoft.AspNetCore.Authentication.Cookies;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(
                builder.Configuration.GetConnectionString("Redis")
            ));

        var rabbit = builder.Configuration.GetSection("RabbitMQ");

        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                HostName = rabbit["Host"],
                UserName = rabbit["Username"],
                Password = rabbit["Password"]
            };

            return factory.CreateConnection();
        });

        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
            });

        builder.Services.AddAuthorization();

        builder.Services.AddRazorPages();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}