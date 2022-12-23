using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Models;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/projects/{projectGuid:guid}/activities")]
public class ActivitiesController  : ApiControllerBase
{
    public ActivitiesController(IApiService apiService, IHttpContextAccessor httpContextAccessor)
        : base(apiService, httpContextAccessor)
    {
    }

    [HttpGet]
    public ActionResult<IAsyncEnumerable<ProjectResourceOutput>> ActivitiesForProjectAsync(Guid projectGuid, CancellationToken ct)
    {
        var projectId = IdOf<Project>.Of(projectGuid);
        var oc = GetOperationContextForCurrentUser();

        var result = ApiService.ActivitiesForProject(projectId, oc, ct);
        return Ok(result);
    }
}