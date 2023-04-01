using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/timesheets")]
public class TimesheetController : ApiControllerBase
{
    public TimesheetController(IApiService apiService, IHttpContextAccessor httpContextAccessor)
        : base(apiService, httpContextAccessor)
    {
    }

    [HttpGet("{isoDate:regex(^\\d{{4}}-\\d{{2}}-\\d{{2}}$)}")]
    public async Task<ActionResult<TimesheetResourceOutput>> Timesheet(string isoDate, CancellationToken ct)
    {
        var dt = DateTime.Parse(isoDate);
        var date = new Date(dt);
        var oc = GetOperationContextForCurrentUser();

        var resource = await ApiService.GetTimesheetAsync(date, oc, ct);
        return Ok(resource);
    }

    [HttpPut("{isoDate:regex(^\\d{{4}}-\\d{{2}}-\\d{{2}}$)}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RegisterTimeAsync(string isoDate, [FromBody] TimesheetResourceInput input, CancellationToken ct)
    {
        var oc = GetOperationContextForCurrentUser();
        var date = Date.From(isoDate);

        // TODO: Assert that date from path is same as date in input
        var output = await ApiService.UpdateTimesheetAsync(input, oc, ct);

        return Ok(output);
    }
}