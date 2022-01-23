using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Queries;
using Quarter.Core.Utils;

namespace Quarter.UnitTest.TestUtils;

public class TestQueryHandler : IQueryHandler
{
    public TimesheetSummaryQueryResult FakeTimesheetSummaryQueryResult { get; set; }

    public Task<TimesheetSummaryQueryResult> ExecuteAsync(TimesheetSummaryQuery query, OperationContext oc, CancellationToken ct)
        => Task.FromResult(FakeTimesheetSummaryQueryResult);
}