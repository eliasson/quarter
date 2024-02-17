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
    private readonly InMemoryUserRepository _userRepository;
    private readonly ConcurrentDictionary<IdOf<User>, IProjectRepository> _projectRepositories;
    private readonly ConcurrentDictionary<IdOf<User>, IActivityRepository> _activityRepositories;
    private readonly ConcurrentDictionary<IdOf<User>, ITimesheetRepository> _timesheetRepositories;

    public InMemoryRepositoryFactory()
    {
        _userRepository = new InMemoryUserRepository();
        _projectRepositories = new ConcurrentDictionary<IdOf<User>, IProjectRepository>();
        _activityRepositories = new ConcurrentDictionary<IdOf<User>, IActivityRepository>();
        _timesheetRepositories = new ConcurrentDictionary<IdOf<User>, ITimesheetRepository>();
    }

    public IUserRepository UserRepository()
        => _userRepository;

    public IProjectRepository ProjectRepository(IdOf<User> userId)
        => _projectRepositories.GetOrAdd(userId, _ => new InMemoryProjectRepository());

    public IActivityRepository ActivityRepository(IdOf<User> userId)
        => _activityRepositories.GetOrAdd(userId, _ => new InMemoryActivityRepository());

    public ITimesheetRepository TimesheetRepository(IdOf<User> userId)
        => _timesheetRepositories.GetOrAdd(userId, _ => new InMemoryTimesheetRepository());
}

public class PostgresRepositoryFactory : IRepositoryFactory
{
    private readonly IPostgresConnectionProvider _connectionProvider;
    private readonly Lazy<IUserRepository> _userRepository;

    public PostgresRepositoryFactory(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
        _userRepository = new Lazy<IUserRepository>(() => new PostgresUserRepository(connectionProvider));
    }

    public IUserRepository UserRepository()
        => _userRepository.Value;

    public IProjectRepository ProjectRepository(IdOf<User> userId)
        => new PostgresProjectRepository(_connectionProvider, userId);

    public IActivityRepository ActivityRepository(IdOf<User> userId)
        => new PostgresActivityRepository(_connectionProvider, userId);

    public ITimesheetRepository TimesheetRepository(IdOf<User> userId)
        => new PostgresTimesheetRepository(_connectionProvider, userId);
}
