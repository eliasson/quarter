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
        var result = ApiService.AllForUserAsync(oc, ct);
        return Ok(result);
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