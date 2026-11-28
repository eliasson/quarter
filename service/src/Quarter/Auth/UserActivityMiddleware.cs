using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Quarter.Core.Auth;
using Quarter.Core.Models;
using Quarter.Services;

namespace Quarter.Auth;

/// <summary>
/// Records activity for authenticated users. Must be added after authentication.
/// </summary>
public class UserActivityMiddleware(RequestDelegate next, ILoginTracker loginTracker)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var idClaim = context.User.FindFirst(ApplicationClaim.QuarterUserIdClaimType);

        if (idClaim is not null && Guid.TryParse(idClaim.Value, out var userGuid))
            await loginTracker.RecordActivityAsync(IdOf<User>.Of(userGuid), context.RequestAborted);

        await next(context);
    }
}
