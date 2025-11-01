using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/projects")]
public class ProjectsController(IApiService apiService, IRepositoryFactory repositoryFactory, IHttpContextAccessor httpContextAccessor)
    : ApiControllerBase(apiService, repositoryFactory, httpContextAccessor)
{
    [HttpGet]
    public ActionResult<IAsyncEnumerable<ProjectResourceOutput>> All(CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var result = ApiService.ProjectsForUserAsync(oc, ct);
        return Ok(result);
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreateProjectAsync([FromBody] CreateProjectResourceInput input, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var output = await ApiService.CreateProjectAsync(input, oc, ct);
        return Created(output.Location(), output);
    }

    [HttpPatch("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateProjectAsync(Guid id, [FromBody] UpdateProjectResourceInput input, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var projectId = IdOf<Project>.Of(id);
        var output = await ApiService.UpdateProjectAsync(projectId, input, oc, ct);
        return Ok(output);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteProjectAsync(Guid id, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        await ApiService.DeleteProjectAsync(IdOf<Project>.Of(id), oc, ct);
        return NoContent();
    }

    [HttpGet("activities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> WithActivities(CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var result = await ApiService.GetAllProjectsAndActivitiesForUserAsync(oc, ct);
        return Ok(result);
    }
}
