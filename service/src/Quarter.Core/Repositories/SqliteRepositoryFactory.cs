using Quarter.Core.Models;

namespace Quarter.Core.Repositories;

public class SqliteRepositoryFactory(ISqliteConnectionProvider connectionProvider) : IRepositoryFactory
{
    private readonly System.Lazy<IUserRepository> _userRepository = new(() => new SqliteUserRepository(connectionProvider));

    public IUserRepository UserRepository()
        => _userRepository.Value;

    public IProjectRepository ProjectRepository(IdOf<User> userId)
        => new SqliteProjectRepository(connectionProvider, userId);

    public IActivityRepository ActivityRepository(IdOf<User> userId)
        => new SqliteActivityRepository(connectionProvider, userId);

    public ITimesheetRepository TimesheetRepository(IdOf<User> userId)
        => new SqliteTimesheetRepository(connectionProvider, userId);
}
