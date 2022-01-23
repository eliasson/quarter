using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Services;

namespace Quarter.UnitTest.TestUtils;

public class TestIUserAuthorizationService : IUserAuthorizationService
{
    public IdOf<User> UserId { get; set; }

    public Task<AuthorizedResult> IsUserAuthorized(string email, CancellationToken ct)
    {
        var result = UserId == null
            ? AuthorizedResult.Unauthorized()
            : AuthorizedResult.AuthorizedWith(Array.Empty<Claim>());
        return Task.FromResult(result);
    }

    public Task<IdOf<User>> CurrentUserId()
        => Task.FromResult(UserId);
}
