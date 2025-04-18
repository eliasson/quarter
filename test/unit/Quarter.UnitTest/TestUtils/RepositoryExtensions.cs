using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Repositories;

namespace Quarter.UnitTest.TestUtils;

public static class RepositoryExtensions
{
    public static async Task TruncateAll(this IRepositoryFactory repoFactory)
    {
        await foreach (var user in repoFactory.UserRepository().GetAllAsync(CancellationToken.None))
        {
            await repoFactory.ProjectRepository(user.Id).Truncate(CancellationToken.None);
            await repoFactory.ActivityRepository(user.Id).Truncate(CancellationToken.None);
            await repoFactory.TimesheetRepository(user.Id).Truncate(CancellationToken.None);
        }

        await repoFactory.UserRepository().Truncate(CancellationToken.None);
    }
}
