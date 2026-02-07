using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi;

[Route("api/timesheets")]
public class TimesheetController(IApiService apiService, IRepositoryFactory repositoryFactory, IHttpContextAccessor httpContextAccessor)
    : ApiControllerBase(apiService, repositoryFactory, httpContextAccessor)
{
    [HttpGet("{isoDate:regex(^\\d{{4}}-\\d{{2}}-\\d{{2}}$)}")]
    public async Task<ActionResult<TimesheetResourceOutput>> Timesheet(string isoDate, CancellationToken ct)
    {
        var dt = DateTime.Parse(isoDate);
        var date = new Date(dt);
        var oc = await GetOperationContextForCurrentUserAsync(ct);

        var resource = await ApiService.GetTimesheetAsync(date, oc, ct);
        return Ok(resource);
    }

    [HttpPut("{isoDate:regex(^\\d{{4}}-\\d{{2}}-\\d{{2}}$)}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RegisterTimeAsync(string isoDate, [FromBody] TimesheetResourceInput input, CancellationToken ct)
    {
        var oc = await GetOperationContextForCurrentUserAsync(ct);
        var date = Date.From(isoDate);

        // TODO: Assert that date from path is same as date in input
        var output = await ApiService.UpdateTimesheetAsync(input, oc, ct);

        return Ok(output);
    }

    [HttpGet("{year:int}/{month:int:min(1):max(12)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TimesheetsResourceOutput>> TimesheetsForMonth(int year, int month, CancellationToken ct)
    {
        var oc = await GetOperationContextForCurrentUserAsync(ct);
        var date = new Date(new DateTime(year, month, 1));
        var output = await ApiService.GetTimesheetsForMonthAsync(date, oc, ct);

        return Ok(output);

    }
}
