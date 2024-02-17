using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Auth;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Authorize]
[ApiController]
public class ApiControllerBase : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IApiService ApiService;

    public ApiControllerBase(IApiService apiService, IHttpContextAccessor httpContextAccessor)
    {
        ApiService = apiService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected OperationContext GetOperationContextForCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null) throw new UnauthorizedAccessException("No principal found on request");

        var idClaim = principal.Claims.FirstOrDefault(c => c.Type == ApplicationClaim.QuarterUserIdClaimType);
        if (idClaim == null) throw new UnauthorizedAccessException($"Could not find claim of type ({ApplicationClaim.QuarterUserIdClaimType}) on principal");

        var userId = IdOf<User>.Of(Guid.Parse(idClaim.Value));
        return new OperationContext(userId);
    }
}
