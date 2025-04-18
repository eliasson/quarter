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
    public ActionResult<IAsyncEnumerable<UserResourceOutput>> All(CancellationToken ct)
    {
        // TODO Ensure user is admin
        var oc = GetOperationContextForCurrentUser();
        if (!oc.Roles.Contains(UserRole.Administrator))
            return Forbid();
        return Ok(new object());
    }
}
