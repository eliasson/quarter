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
}