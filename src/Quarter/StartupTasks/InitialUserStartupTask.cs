using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Options;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.StartupTasks;

/// <summary>
/// Create the initial user at application startup (if configured).
/// </summary>
public class InitialUserStartupTask : IStartupTask
{
    private readonly ILogger<InitialUserStartupTask> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ICommandHandler _commandHandler;
    private readonly InitialUserOptions? _options;

    public InitialUserStartupTask(
        ILogger<InitialUserStartupTask> logger,
        IOptions<InitialUserOptions> options,
        IRepositoryFactory repositoryFactory,
        ICommandHandler commandHandler
    )
    {
        _logger = logger;
        _userRepository = repositoryFactory.UserRepository();
        _commandHandler = commandHandler;
        _options = options.Value;
    }

    public async Task ExecuteAsync()
    {
        if (_options is { Enabled: true } && _options.Email.Length > 0)
        {
            // TODO: Add repository method HasUserWithEmail
            try
            {
                await _userRepository.GetUserByEmailAsync(_options.Email, CancellationToken.None);
                _logger.LogInformation("Initial user is existing");
            }
            catch (NotFoundException)
            {
                var cmd = new AddUserCommand(new Email(_options.Email), new[] { UserRole.Administrator });
                await _commandHandler.ExecuteAsync(cmd, OperationContext.None, CancellationToken.None);
                _logger.LogDebug("Created initial user with email {Email}", _options.Email);
            }
        }
        else
        {
            _logger.LogInformation("No initial user configured");
        }

    }
}
