using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Quarter.Auth;
using Quarter.Core.Auth;
using Quarter.Core.Models;

namespace Quarter.UnitTest.TestUtils;

public class TestAuthenticationStateProvider : AuthenticationStateProvider
{
    private Claim _claim;

    public void SetCurrentUser(IdOf<User> userId)
    {
        if (userId is { })
            _claim = ApplicationClaim.FromUserId(userId);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claimsPrincipal = new ClaimsPrincipal();
        if (_claim is { })
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new[] { _claim }));
        var state = new AuthenticationState(claimsPrincipal);
        return Task.FromResult(state);
    }
}
