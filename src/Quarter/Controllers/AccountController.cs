using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Quarter.Controllers;

// Authentication is manually tested.
[Route("[controller]")]
public class AccountController(ILogger<AccountController> logger) : Controller
{
    [HttpGet("login")]
    public IActionResult LoginAsync()
        => View("Login");

    [HttpGet("login/github")]
    public IActionResult LoginGitHub()
    {
        logger.LogInformation("Redirecting visitor to GitHub login");
        return Challenge(new AuthenticationProperties { RedirectUri = "/app" }, "GitHub");
    }

    [HttpGet("login/google")]
    public IActionResult LoginGoogleAsync()
    {
        logger.LogInformation("Redirecting visitor to Google login");
        return Challenge(new AuthenticationProperties { RedirectUri = "/app" }, "Google");
    }

    [HttpGet("logout")]
    [HttpPost("logout")]
    public IActionResult Logout()
        => SignOut(new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme);
}
