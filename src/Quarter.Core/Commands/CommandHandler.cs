using System;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Events;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Core.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IEventDispatcher _eventDispatcher;

        public CommandHandler(IRepositoryFactory repositoryFactory, IEventDispatcher eventDispatcher)
        {
            _repositoryFactory = repositoryFactory;
            _eventDispatcher = eventDispatcher;
        }

        public Task ExecuteAsync(ICommand command, OperationContext oc, CancellationToken ct)
        {
            return command switch
            {
                AddUserCommand cmd => ExecuteAsync(cmd, ct),
                RemoveUserCommand cmd => ExecuteAsync(cmd, ct),
                AssignUserRoleCommand cmd => ExecuteAsync(cmd, ct),
                RevokeUserRoleCommand cmd => ExecuteAsync(cmd, ct),
                AddProjectCommand cmd => ExecuteAsync(cmd, oc ,ct),
                EditProjectCommand cmd => ExecuteAsync(cmd, oc ,ct),
                RemoveProjectCommand cmd => ExecuteAsync(cmd, oc, ct),
                AddActivityCommand cmd => ExecuteAsync(cmd, oc, ct),
                EditActivityCommand cmd => ExecuteAsync(cmd, oc, ct),
                RemoveActivityCommand cmd => ExecuteAsync(cmd, oc, ct),
                _ => throw new NotImplementedException(),
            };
        }

        private async Task ExecuteAsync(AddUserCommand command, CancellationToken ct)
        {
            var user = new User(command.Email, command.Roles);
            user = await _repositoryFactory.UserRepository().CreateAsync(user, ct);
            await _eventDispatcher.Dispatch(new UserCreatedEvent(user));
        }

        private async Task ExecuteAsync(RemoveUserCommand command, CancellationToken ct)
        {
            var result = await _repositoryFactory.UserRepository().RemoveByIdAsync(command.UserId, ct);
            if (result == RemoveResult.Removed)
                await _eventDispatcher.Dispatch(new UserRemovedEvent(command.UserId));
        }

        private async Task ExecuteAsync(AssignUserRoleCommand command, CancellationToken ct)
        {
            await _repositoryFactory.UserRepository()
                .UpdateByIdAsync(command.UserId, user =>
                {
                    user.AssignRole(command.Role);
                    return user;
                }, ct);

            await _eventDispatcher.Dispatch(new AssignedUserRoleEvent(command.UserId, new [] { command.Role }));
        }

        private async Task ExecuteAsync(RevokeUserRoleCommand command, CancellationToken ct)
        {
            await _repositoryFactory.UserRepository()
                .UpdateByIdAsync(command.UserId, user =>
                {
                    user.RevokeRole(command.Role);
                    return user;
                }, ct);

            await _eventDispatcher.Dispatch(new RevokedUserRoleEvent(command.UserId, new [] { command.Role }));
        }

        private async Task ExecuteAsync(AddProjectCommand command, OperationContext oc, CancellationToken ct)
        {
            var project = new Project(command.Name, command.Description);
            project = await _repositoryFactory.ProjectRepository(oc.UserId).CreateAsync(project, ct);
            await _eventDispatcher.Dispatch(new ProjectCreatedEvent(project));
        }

        private async Task ExecuteAsync(EditProjectCommand command, OperationContext oc, CancellationToken ct)
        {
            await _repositoryFactory.ProjectRepository(oc.UserId).UpdateByIdAsync(command.ProjectId,
                current =>
                {
                    if (command.Name is not null)
                        current.Name = command.Name;
                    if (command.Description is not null)
                        current.Description = command.Description;

                    return current;
                }, ct);

            await _eventDispatcher.Dispatch(new ProjectEditedEvent(command.ProjectId));
        }

        private async Task ExecuteAsync(RemoveProjectCommand command, OperationContext oc, CancellationToken ct)
        {
            var result = await _repositoryFactory.ProjectRepository(oc.UserId).RemoveByIdAsync(command.ProjectId, ct);
            if (result == RemoveResult.Removed)
                await _eventDispatcher.Dispatch(new ProjectRemovedEvent(command.ProjectId));
        }

        private async Task ExecuteAsync(AddActivityCommand command, OperationContext oc, CancellationToken ct)
        {
            var activity = new Activity(command.ProjectId, command.Name, command.Description, command.Color);
            activity = await _repositoryFactory.ActivityRepository(oc.UserId).CreateAsync(activity, ct);
            await _eventDispatcher.Dispatch(new ActivityCreatedEvent(activity));
        }

        private async Task ExecuteAsync(EditActivityCommand command, OperationContext oc, CancellationToken ct)
        {
            await _repositoryFactory.ActivityRepository(oc.UserId).UpdateByIdAsync(command.ActivityId, current =>
            {
                if (command.Name is not null)
                    current.Name = command.Name;
                if (command.Description is not null)
                    current.Description = command.Description;
                if (command.Color is not null)
                    current.Color = command.Color;

                return current;
            }, ct);
            await _eventDispatcher.Dispatch(new ActivityEditedEvent(command.ActivityId));
        }

        private async Task ExecuteAsync(RemoveActivityCommand command, OperationContext oc, CancellationToken ct)
        {
            var result = await _repositoryFactory.ActivityRepository(oc.UserId).RemoveByIdAsync(command.ActivityId, ct);
            if (result == RemoveResult.Removed)
            {
                await _repositoryFactory.TimesheetRepository(oc.UserId).RemoveSlotsForActivityAsync(command.ActivityId, ct);
                await _eventDispatcher.Dispatch(new ActivityRemovedEvent(command.ActivityId));
            }
        }
    }
}