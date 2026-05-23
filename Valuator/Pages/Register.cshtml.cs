using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Text.Json;
using Valuator.Models;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly IConnectionMultiplexer _redis;

    public RegisterModel(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public IActionResult OnPost(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(password))
        {
            return Page();
        }

        var db = _redis.GetDatabase();

        string key = "USER-" + login;

        if (db.KeyExists(key))
        {
            return Page();
        }

        var user = new User
        {
            Login = login,
            Password = BCrypt.Net.BCrypt.HashPassword(password)
        };

        db.StringSet(
            key,
            JsonSerializer.Serialize(user)
        );

        return RedirectToPage("/Login");
    }
}