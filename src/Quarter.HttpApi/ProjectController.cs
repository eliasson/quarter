using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Models;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/[controller]")]
public class ProjectController : ApiControllerBase
{
    public ProjectController(IApiService apiService, IHttpContextAccessor httpContextAccessor)
        : base(apiService, httpContextAccessor)
    {
    }

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
    public async Task<ActionResult> CreateProjectAsync([FromBody] ProjectResourceInput input, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var output = await ApiService.CreateProjectAsync(input, oc, ct);
        return Created(output.Location(), output);
    }

    [HttpPut("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateProjectAsync(Guid id, [FromBody] ProjectResourceInput input, CancellationToken ct)
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
}