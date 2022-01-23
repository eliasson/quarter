using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Core.Queries;

public interface IQueryHandler
{
    Task<TimesheetSummaryQueryResult> ExecuteAsync(TimesheetSummaryQuery query, OperationContext oc, CancellationToken ct);
}

public class QueryHandler : IQueryHandler
{
    private readonly IRepositoryFactory _repositoryFactory;

    public QueryHandler(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task<TimesheetSummaryQueryResult> ExecuteAsync(TimesheetSummaryQuery query, OperationContext oc, CancellationToken ct)
    {
        var timesheetRepository = _repositoryFactory.TimesheetRepository(oc.UserId);
        var vm =  new TimesheetSummaryQueryResult();

        // This is inefficient but I currently cannot be bothered to add a repository method to do this in a single
        // query right now.
        // TODO: Add repository method to get multiple timesheets for range of dates
        foreach (var date in Date.Sequence(query.From, query.To))
        {
            var timesheet = await timesheetRepository.GetOrNewTimesheetAsync(date, ct);
            vm.Add(timesheet);
        }
        return vm;
    }
}