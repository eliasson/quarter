using System;
using System.Collections.Concurrent;
using Quarter.Core.Models;

namespace Quarter.Core.Repositories;

public interface IRepositoryFactory
{
    IUserRepository UserRepository();
    IProjectRepository ProjectRepository(IdOf<User> userId);
    IActivityRepository ActivityRepository(IdOf<User> userId);
    ITimesheetRepository TimesheetRepository(IdOf<User> userId);
}

public class InMemoryRepositoryFactory : IRepositoryFactory
{
    private readonly InMemoryUserRepository _userRepository = new();
    private readonly ConcurrentDictionary<IdOf<User>, IProjectRepository> _projectRepositories = new();
    private readonly ConcurrentDictionary<IdOf<User>, IActivityRepository> _activityRepositories = new();
    private readonly ConcurrentDictionary<IdOf<User>, ITimesheetRepository> _timesheetRepositories = new();

    public IUserRepository UserRepository()
        => _userRepository;

    public IProjectRepository ProjectRepository(IdOf<User> userId)
        => _projectRepositories.GetOrAdd(userId, _ => new InMemoryProjectRepository());

    public IActivityRepository ActivityRepository(IdOf<User> userId)
        => _activityRepositories.GetOrAdd(userId, _ => new InMemoryActivityRepository());

    public ITimesheetRepository TimesheetRepository(IdOf<User> userId)
        => _timesheetRepositories.GetOrAdd(userId, _ => new InMemoryTimesheetRepository());
}

public class PostgresRepositoryFactory(IPostgresConnectionProvider connectionProvider) : IRepositoryFactory
{
    private readonly Lazy<IUserRepository> _userRepository = new(() => new PostgresUserRepository(connectionProvider));

    public IUserRepository UserRepository()
        => _userRepository.Value;

    public IProjectRepository ProjectRepository(IdOf<User> userId)
        => new PostgresProjectRepository(connectionProvider, userId);

    public IActivityRepository ActivityRepository(IdOf<User> userId)
        => new PostgresActivityRepository(connectionProvider, userId);

    public ITimesheetRepository TimesheetRepository(IdOf<User> userId)
        => new PostgresTimesheetRepository(connectionProvider, userId);
}
