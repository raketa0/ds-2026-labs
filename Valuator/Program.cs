<<<<<<< HEAD
=======
using StackExchange.Redis;

>>>>>>> pa1
namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
        // Add services to the container.
=======
        // Add services to the container. Добавил
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis") 
        ));

>>>>>>> pa1
        builder.Services.AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
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
