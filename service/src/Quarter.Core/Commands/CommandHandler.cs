using System;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Core.Commands;

public class CommandHandler(IRepositoryFactory repositoryFactory) : ICommandHandler
{
    public Task ExecuteAsync(ICommand command, OperationContext oc, CancellationToken ct)
    {
        return command switch
        {
            AddUserCommand cmd => ExecuteAsync(cmd, ct),
            _ => throw new NotImplementedException(),
        };
    }

    private async Task ExecuteAsync(AddUserCommand command, CancellationToken ct)
    {
        var user = new User(command.Email, command.Roles);
        user = await repositoryFactory.UserRepository().CreateAsync(user, ct);

        // Create placeholder project and activity to the user has something to begin with
        var project = await repositoryFactory.ProjectRepository(user.Id).CreateSandboxProjectAsync(ct);
        _ = await repositoryFactory.ActivityRepository(user.Id).CreateSandboxActivityAsync(project.Id, ct);
    }
}
