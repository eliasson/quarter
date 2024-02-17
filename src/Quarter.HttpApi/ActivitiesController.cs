using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Models;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/projects/{projectGuid:guid}/activities")]
public class ActivitiesController : ApiControllerBase
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

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreateActivityAsync(Guid projectGuid, [FromBody] CreateActivityResourceInput input, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var projectId = IdOf<Project>.Of(projectGuid);

        var output = await ApiService.CreateActivityAsync(projectId, input, oc, ct);
        return Created(output.Location(), output);
    }

    [HttpPatch("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateActivityAsync(Guid projectGuid, Guid id, [FromBody] UpdateActivityResourceInput input, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var projectId = IdOf<Project>.Of(projectGuid);
        var activityId = IdOf<Activity>.Of(id);

        var output = await ApiService.UpdateActivityAsync(projectId, activityId, input, oc, ct);
        return Ok(output);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteActivityAsync(Guid projectGuid, Guid id, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var projectId = IdOf<Project>.Of(projectGuid);
        var activityId = IdOf<Activity>.Of(id);

        await ApiService.DeleteActivityAsync(projectId, activityId, oc, ct);
        return NoContent();
    }
}
