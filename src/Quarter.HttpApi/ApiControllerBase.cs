using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Auth;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Authorize]
[ApiController]
public class ApiControllerBase(IApiService apiService, IRepositoryFactory repositoryFactory, IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    protected readonly IApiService ApiService = apiService;

    protected OperationContext GetOperationContextForCurrentUser()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal == null) throw new UnauthorizedAccessException("No principal found on request");

        var idClaim = principal.Claims.FirstOrDefault(c => c.Type == ApplicationClaim.QuarterUserIdClaimType);
        if (idClaim == null) throw new UnauthorizedAccessException($"Could not find claim of type ({ApplicationClaim.QuarterUserIdClaimType}) on principal");

        var userId = IdOf<User>.Of(Guid.Parse(idClaim.Value));

        return new OperationContext(userId, []);
    }

    // TODO Should we do this in a middleware or something instead?
    protected Task<User> GetCurrentUserAsync(OperationContext oc, CancellationToken ct)
        => repositoryFactory.UserRepository().GetByIdAsync(oc.UserId, ct);
}
