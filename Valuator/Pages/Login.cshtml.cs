using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
using Valuator.Models;

namespace Valuator.Pages;

public class LoginModel : PageModel
{
    private readonly IConnectionMultiplexer _redis;

    public LoginModel(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IActionResult> OnPost(
        string login,
        string password)
    {
        var db = _redis.GetDatabase();

        string key = "USER-" + login;

        var json = db.StringGet(key);

        if (json.IsNull)
        {
            return Page();
        }

        var user = JsonSerializer.Deserialize<User>(json!);

        if (user == null)
        {
            return Page();
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, login)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return RedirectToPage("/Index");
    }
}