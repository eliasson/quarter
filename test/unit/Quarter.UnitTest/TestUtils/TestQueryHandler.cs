using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.UnitTest.TestUtils;

public class TestQueryHandler : IQueryHandler
{
    public TimesheetSummaryQueryResult FakeTimesheetSummaryQueryResult { get; set; }
    public WeeklyReportResult FakeWeeklyReportResult { get; set; }
    public UsageOverTime FakeMonthlyReportResult { get; set; }

    public Task<TimesheetSummaryQueryResult> ExecuteAsync(TimesheetSummaryQuery query, OperationContext oc, CancellationToken ct)
        => Task.FromResult(FakeTimesheetSummaryQueryResult);

    public Task<WeeklyReportResult> ExecuteAsync(WeeklyReportQuery query, OperationContext oc, CancellationToken ct)
        => Task.FromResult(FakeWeeklyReportResult);

    public Task<UsageOverTime> ExecuteAsync(MonthlyReportQuery query, OperationContext oc, CancellationToken ct)
        => Task.FromResult(FakeMonthlyReportResult);
}