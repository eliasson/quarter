using System.Security.Claims;
using Quarter.Core.Models;

namespace Quarter.Core.Auth;

public static class ApplicationClaim
{
    public const string QuarterUserIdClaimType = "quarter-user-id";

    public static Claim FromUserId(IdOf<User> userId)
        => new(QuarterUserIdClaimType, userId.AsString());
}
