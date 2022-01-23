using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Queries;

public class QueryTestBase
{
    protected readonly IQueryHandler QueryHandler;
    protected readonly InMemoryRepositoryFactory RepositoryFactory;
    protected IdOf<User> ActingUser = IdOf<User>.Random();

    protected QueryTestBase()
    {
        RepositoryFactory = new InMemoryRepositoryFactory();
        QueryHandler = new QueryHandler(RepositoryFactory);
    }

    protected OperationContext OperationContext()
        => new OperationContext(ActingUser);

    protected async Task StoreTimesheet(IdOf<User> userId, Timesheet timesheet)
    {
        var timesheetRepository = RepositoryFactory.TimesheetRepository(userId);
        await timesheetRepository.CreateAsync(timesheet, CancellationToken.None);
    }
}
