using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/reports")]
public class ReportsController(IApiService apiService, IRepositoryFactory repositoryFactory, IHttpContextAccessor httpContextAccessor)
    : ApiControllerBase(apiService, repositoryFactory, httpContextAccessor)
{
    [HttpGet("week/{isoDate:regex(^\\d{{4}}-\\d{{2}}-\\d{{2}}$)}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TimesheetResourceOutput>> WeeklyReportAsync(string isoDate, CancellationToken ct)
    {
        var dt = DateTime.Parse(isoDate);
        var date = new Date(dt);
        var oc = await GetOperationContextForCurrentUserAsync(ct);

        var resource = await ApiService.GetWeeklyReportAsync(date, oc, ct);
        return Ok(resource);
    }
}
