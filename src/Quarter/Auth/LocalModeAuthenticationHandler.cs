using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quarter.Core.Auth;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.Services;

namespace Quarter.Auth;

/// <summary>
/// A fake authorization service that use a fix User for all sessions, basically shortcutting auth.
///
/// Usable during development (or if someone wants to run this solo, on local host, for whatever reason)
/// </summary>
public class LocalAuthorizationService : IUserAuthorizationService
{
    public Task<IdOf<User>> CurrentUserId()
        => Task.FromResult(LocalUser.UserId)!;

    public Task<AuthorizedResult> IsUserAuthorized(string email, CancellationToken ct)
        => Task.FromResult(AuthorizedResult.AuthorizedWith(LocalUser.Claims));
}

public class LocalModeAuthenticationHandler : AuthenticationHandler<LocalModeAuthenticationOptions>
{
    public LocalModeAuthenticationHandler(IOptionsMonitor<LocalModeAuthenticationOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        var identity = new ClaimsIdentity(LocalUser.Claims, nameof(LocalModeAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// When running the application in local mode, this is the user that is provided as an authenticated user.
/// </summary>
public static class LocalUser
{
    private const string StaticUserGuid = "47ba567a-711e-4c4a-a7b0-07756d965a79";
    private static readonly Guid UserIdGuid = Guid.Parse(StaticUserGuid);
    public static readonly IdOf<User> UserId = IdOf<User>.Of(UserIdGuid);
    public static readonly User User;

    static LocalUser()
    {
        User = new User(new Email("local@quarterapp.com"), new [] { UserRole.Administrator })
        {
            Id = UserId
        };
    }

    public static readonly Claim[] Claims = new[]
    {
        ApplicationClaim.FromUserId(UserId),
        new Claim(ClaimTypes.Name, "Local user"),
    };
}

public class LocalModeAuthenticationOptions : AuthenticationSchemeOptions
{
}
