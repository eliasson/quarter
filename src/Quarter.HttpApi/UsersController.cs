using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/users")]
public class UsersController(IApiService apiService, IRepositoryFactory repositoryFactory, IHttpContextAccessor httpContextAccessor)
    : ApiControllerBase(apiService, repositoryFactory, httpContextAccessor)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResourceOutput>>> All(CancellationToken ct)
    {
        var oc = await GetOperationContextForCurrentUserAsync(ct);
        if (!oc.HasRole(UserRole.Administrator))
            return Forbid();

        var users = ApiService.GetAllUsersAsync(oc, ct);
        return Ok(users);
    }
}
