using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quarter.Auth;
using Quarter.Core.Exceptions;
using Quarter.Core.Repositories;

namespace Quarter.StartupTasks;

/// <summary>
/// Ensures the hardcoded local mode user exists in the database at startup.
/// Without this, every API request in local mode fails because the authentication
/// handler provides a user ID that does not exist in the database.
/// </summary>
public class LocalUserStartupTask(
    ILogger<LocalUserStartupTask> logger,
    IRepositoryFactory repositoryFactory)
    : IStartupTask
{
    private readonly IUserRepository _userRepository = repositoryFactory.UserRepository();

    public async Task ExecuteAsync()
    {
        try
        {
            await _userRepository.GetByIdAsync(LocalUser.UserId, CancellationToken.None);
            logger.LogInformation("Local mode user already exists");
        }
        catch (NotFoundException)
        {
            await _userRepository.CreateAsync(LocalUser.User, CancellationToken.None);
            logger.LogInformation("Created local mode user {Email}", LocalUser.User.Email);
        }
    }
}
