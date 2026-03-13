using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Components.Modals;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.UI.State;
using Quarter.Core.Utils;
using Quarter.Pages.Admin.Users;
using Quarter.Pages.Application.Manage;
using Quarter.Services;
using Quarter.State.Forms;
using Quarter.State.ViewModels;

namespace Quarter.State;

public class ActionHandler(
    IRepositoryFactory repositoryFactory,
    ICommandHandler commandHandler,
    IUserAuthorizationService userAuthorizationService)
    : IActionHandler<ApplicationState>
{
    public async Task<ApplicationState> HandleAsync(ApplicationState currentState, IAction action, CancellationToken ct)
    {
        currentState.StateChanges += 1;

        var task = action switch
        {
            // Misc actions
            CloseModalAction a => HandleAsync(currentState, a, ct),
            ShowAddUserAction a => HandleAsync(currentState, a, ct),
            ShowRemoveUserAction a => HandleAsync(currentState, a, ct),
            ConfirmRemoveUserAction a => HandleAsync(currentState, a, ct),
            AddUserAction a => HandleAsync(currentState, a, ct),

            // Project related actions
            LoadProjects a => HandleAsync(currentState, a, ct),
            ShowAddProjectAction a => HandleAsync(currentState, a, ct),
            AddProjectAction a => HandleAsync(currentState, a, ct),
            ShowRemoveProjectAction a => HandleAsync(currentState, a, ct),
            ConfirmRemoveProjectAction a => HandleAsync(currentState, a, ct),
            ShowEditProjectAction a => HandleAsync(currentState, a, ct),
            EditProjectAction a => HandleAsync(currentState, a, ct),
            ShowArchiveProjectAction a => HandleAsync(currentState, a, ct),
            ConfirmArchiveProjectAction a => HandleAsync(currentState, a, ct),
            ShowRestoreProjectAction a => HandleAsync(currentState, a, ct),
            ConfirmRestoreProjectAction a => HandleAsync(currentState, a, ct),

            // Activity related actions
            ShowAddActivityAction a => HandleAsync(currentState, a, ct),
            AddActivityAction a => HandleAsync(currentState, a, ct),
            ShowRemoveActivityAction a => HandleAsync(currentState, a, ct),
            ConfirmRemoveActivityAction a => HandleAsync(currentState, a, ct),
            ShowEditActivityAction a => HandleAsync(currentState, a, ct),
            EditActivityAction a => HandleAsync(currentState, a, ct),
            ShowArchiveActivityAction a => HandleAsync(currentState, a, ct),
            ConfirmArchiveActivityAction a => HandleAsync(currentState, a, ct),
            ShowRestoreActivityAction a => HandleAsync(currentState, a, ct),
            ConfirmRestoreActivityAction a => HandleAsync(currentState, a, ct),

            // Timesheet related actions
            LoadTimesheetAction a => HandleAsync(currentState, a, ct),
            SelectEraseActivityAction a => HandleAsync(currentState, a, ct),
            SelectActivityAction a => HandleAsync(currentState, a, ct),
            TimeAction a => HandleAsync(currentState, a, ct),
            ExtendStartOfDay a => HandleAsync(currentState, a, ct),
            ExtendEndOfDay a => HandleAsync(currentState, a, ct),

            _ => Task.FromResult(currentState) // This should be exhaustive - why is this required!
        };
        return await task;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, CloseModalAction action, CancellationToken ct)
    {
        currentState.Modals.Pop();
        return Task.FromResult(currentState);
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, SelectEraseActivityAction action, CancellationToken ct)
    {
        currentState.SelectedActivity = null;
        return Task.FromResult(currentState);
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, SelectActivityAction action, CancellationToken ct)
    {
        currentState.SelectedActivity = action.SelectedActivity;
        return Task.FromResult(currentState);
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowAddUserAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(UserModal), new Dictionary<string, object>
            {
                { ApplicationState.FormData, new UserFormData() },
            }));
        return Task.FromResult(currentState);
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowRemoveUserAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ConfirmModal), new Dictionary<string, object>
            {
                { nameof(ConfirmModal.Title), "Remove user?" },
                {
                    nameof(ConfirmModal.Message),
                    "Are you sure you want to remove this user and all associated projects? This cannot be undone!"
                },
                { nameof(ConfirmModal.ConfirmText), "Remove" },
                { nameof(ConfirmModal.IsDangerous), true },
                { nameof(ConfirmModal.OnConfirmAction), new ConfirmRemoveUserAction(action.UserId) }
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ConfirmRemoveUserAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new RemoveUserCommand(IdOf<User>.Of(action.UserId));
        await commandHandler.ExecuteAsync(command, oc, ct);

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, AddUserAction action, CancellationToken ct)
    {
        var roles = action.FormData.IsAdmin
            ? new[] { UserRole.Administrator }
            : Array.Empty<UserRole>();

        var oc = await OperationContextForCurrentUser();
        var command = new AddUserCommand(new Email(action.FormData.Email), roles);
        await commandHandler.ExecuteAsync(command, oc, ct);

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, LoadProjects action, CancellationToken ct)
    {
        if (currentState.Projects.Any() && !action.Force)
            return currentState;

        var oc = await OperationContextForCurrentUser();
        var projectRepository = repositoryFactory.ProjectRepository(oc.UserId);
        var projects = await projectRepository.GetAllAsync(ct).ToListAsync(ct);
        var activitiesPerProject = await GetActivitiesPerProjectAsync(oc.UserId, ct);
        var timesheetRepository = repositoryFactory.TimesheetRepository(oc.UserId);

        var vms = new List<ProjectViewModel>();
        foreach (var project in projects)
        {
            var usage = await timesheetRepository.GetProjectTotalUsageAsync(project.Id, ct);
            vms.Add(FromProject(project, activitiesPerProject, usage));
        }

        currentState.Projects = vms;


        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowAddProjectAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ProjectModal), new Dictionary<string, object>
            {
                { ApplicationState.FormData, new ProjectFormData() },
                { ApplicationState.ModalTitle, "Add project" },
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, AddProjectAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new AddProjectCommand(action.FormData.Name, action.FormData.Description);
        await commandHandler.ExecuteAsync(command, oc, ct);

        // This is a bit ugly, but there is (currently) no way to get the ID of the project just created (this is
        // required for sub-sequent actions as edit, delete, add activity).
        // This could be solved by either:
        //
        // - Letting CommandHandler.ExecuteAsync return the contextual IdOf<T> (when applicable).
        // - Subscribing to the event dispatcher (which in turn requires a new way to invoke a new state update)
        //
        // For now, just fetch all projects and add the new one(s), since these are new there cannot be any related activities.
        // Manually tested!!
        var existingIds = currentState.Projects.Select(p => p.Id).ToHashSet();
        var allProjects = await repositoryFactory.ProjectRepository(oc.UserId).GetAllAsync(ct)
            .ToListAsync(ct);
        var noActivities = new Dictionary<IdOf<Project>, IList<Activity>>();
        foreach (var project in allProjects)
        {
            if (!existingIds.Contains(project.Id))
                currentState.Projects.Add(FromProject(project, noActivities, ProjectTotalUsage.Zero));
        }

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowRemoveProjectAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ConfirmModal), new Dictionary<string, object>
            {
                { nameof(ConfirmModal.Title), "Remove project?" },
                {
                    nameof(ConfirmModal.Message),
                    "Are you sure you want to remove this project and all associated activities? This cannot be undone!"
                },
                { nameof(ConfirmModal.ConfirmText), "Remove" },
                { nameof(ConfirmModal.IsDangerous), true },
                { nameof(ConfirmModal.OnConfirmAction), new ConfirmRemoveProjectAction(action.ProjectId) }
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ConfirmRemoveProjectAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new RemoveProjectCommand(action.ProjectId);
        await commandHandler.ExecuteAsync(command, oc, ct);

        var project = currentState.Projects.First(p => Equals(p.Id, command.ProjectId));
        currentState.Projects.Remove(project);

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowEditProjectAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var project = await repositoryFactory.ProjectRepository(oc.UserId)
            .GetByIdAsync(action.ProjectId, ct);
        currentState.Modals.Push(
            new ModalState(typeof(ProjectModal), new Dictionary<string, object>
            {
                {
                    ApplicationState.FormData, new ProjectFormData
                    {
                        Name = project.Name,
                        Description = project.Description,
                    }
                },
                { ApplicationState.ModalTitle, "Edit project" },
                { nameof(ProjectModal.ProjectId), action.ProjectId }
            }));
        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, EditProjectAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new EditProjectCommand(action.ProjectId, action.FormData.Name, action.FormData.Description);
        await commandHandler.ExecuteAsync(command, oc, ct);

        // Update the project in place to avoid re-reading
        var project = currentState.Projects.Single(p => p.Id == action.ProjectId);
        project.Name = action.FormData.Name;
        project.Description = action.FormData.Description;

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowArchiveProjectAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ConfirmModal), new Dictionary<string, object>
            {
                { nameof(ConfirmModal.Title), "Archive project?" },
                {
                    nameof(ConfirmModal.Message),
                    "If you archive this project it can no longer be used to register time. All registered time will still be available though. This project can be restored at a later time."
                },
                { nameof(ConfirmModal.ConfirmText), "Archive" },
                {
                    nameof(ConfirmModal.OnConfirmAction), new ConfirmArchiveProjectAction(action.ProjectId)
                }
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ConfirmArchiveProjectAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new ArchiveProjectCommand(action.ProjectId);
        await commandHandler.ExecuteAsync(command, oc, ct);

        // Update the project in place to avoid re-reading
        var project = currentState.Projects.Single(p => p.Id == action.ProjectId);
        project.IsArchived = true;

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowRestoreProjectAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ConfirmModal), new Dictionary<string, object>
            {
                { nameof(ConfirmModal.Title), "Restore project?" },
                {
                    nameof(ConfirmModal.Message),
                    "If you restore this project you will be able to use it to register time again. All previously registered time will still be available. The project can later be archived again."
                },
                { nameof(ConfirmModal.ConfirmText), "Restore" },
                {
                    nameof(ConfirmModal.OnConfirmAction), new ConfirmRestoreProjectAction(action.ProjectId)
                }
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ConfirmRestoreProjectAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new RestoreProjectCommand(action.ProjectId);
        await commandHandler.ExecuteAsync(command, oc, ct);

        // Update the project in place to avoid re-reading
        var project = currentState.Projects.Single(p => p.Id == action.ProjectId);
        project.IsArchived = false;

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowAddActivityAction a, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ActivityModal), new Dictionary<string, object>
            {
                { ApplicationState.FormData, new ActivityFormData() },
                { ApplicationState.ModalTitle, "Add activity" },
                { nameof(ActivityModal.ProjectId), a.ProjectId },
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, AddActivityAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var color = Color.FromHexString(action.FormData.Color);
        var command = new AddActivityCommand(action.ProjectId, action.FormData.Name, action.FormData.Description,
            color);

        await commandHandler.ExecuteAsync(command, oc, ct);

        // This is the same ugly workaround as in `HandleAddProjectAction`.
        // The activity ID is unknown, hence we must re-read all activities and add the new one(s) to the given project
        //
        // MANUALLY TESTED!
        var activities = await GetActivitiesForProjectAsync(oc.UserId, action.ProjectId, ct);
        var project = currentState.Projects.First(p => Equals(p.Id, action.ProjectId));
        var existingIds = project.Activities.Select(a => a.Id).ToHashSet();

        foreach (var activity in activities)
        {
            if (!existingIds.Contains(activity.Id))
                project.Activities.Add(FromActivity(activity, ProjectTotalUsage.Zero));
        }

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowRemoveActivityAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ConfirmModal), new Dictionary<string, object>
            {
                { nameof(ConfirmModal.Title), "Remove activity?" },
                {
                    nameof(ConfirmModal.Message),
                    "Are you sure you want to remove this activity and all registered time? This cannot be undone!"
                },
                { nameof(ConfirmModal.ConfirmText), "Remove" },
                { nameof(ConfirmModal.IsDangerous), true },
                { nameof(ConfirmModal.OnConfirmAction), new ConfirmRemoveActivityAction(action.ActivityId) }
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ConfirmRemoveActivityAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new RemoveActivityCommand(action.ActivityId);
        await commandHandler.ExecuteAsync(command, oc, ct);

        // TODO: Add projectId to the action to make this lookup faster
        foreach (var p in currentState.Projects)
        {
            foreach (var a in p.Activities)
            {
                if (Equals(a.Id, command.ActivityId))
                {
                    p.Activities.Remove(a);
                    break;
                }
            }
        }

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowEditActivityAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var activity = await repositoryFactory.ActivityRepository(oc.UserId)
            .GetByIdAsync(action.ActivityId, ct);
        currentState.Modals.Push(
            new ModalState(typeof(ActivityModal), new Dictionary<string, object>
            {
                {
                    ApplicationState.FormData, new ActivityFormData
                    {
                        Name = activity.Name,
                        Description = activity.Description,
                        Color = activity.Color.ToHex(),
                    }
                },
                { ApplicationState.ModalTitle, "Edit activity" },
                { nameof(ActivityModal.ProjectId), action.ProjectId },
                { nameof(ActivityModal.ActivityId), action.ActivityId }
            }));
        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, EditActivityAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new EditActivityCommand(action.ActivityId, action.FormData.Name, action.FormData.Description, Color.FromHexString(action.FormData.Color));
        await commandHandler.ExecuteAsync(command, oc, ct);

        // Update the activity in place to avoid re-reading
        var activity = currentState.Projects.Single(p => p.Id == action.ProjectId)
            .Activities.Single(a => a.Id == action.ActivityId);
        activity.Name = action.FormData.Name;
        activity.Description = action.FormData.Description;
        activity.Color = action.FormData.Color;

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowArchiveActivityAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ConfirmModal), new Dictionary<string, object>
            {
                { nameof(ConfirmModal.Title), "Archive activity?" },
                {
                    nameof(ConfirmModal.Message),
                    "If you archive this activity it can no longer be used to register time. All registered time will still be available though. This activity can be restored at a later time."
                },
                { nameof(ConfirmModal.ConfirmText), "Archive" },
                {
                    nameof(ConfirmModal.OnConfirmAction), new ConfirmArchiveActivityAction(action.ActivityId)
                }
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ConfirmArchiveActivityAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new ArchiveActivityCommand(action.ActivityId);
        await commandHandler.ExecuteAsync(command, oc, ct);

        // TODO: Add projectId to the action to make this lookup faster
        foreach (var p in currentState.Projects)
        {
            foreach (var a in p.Activities)
            {
                if (Equals(a.Id, command.ActivityId))
                {
                    a.IsArchived = true;
                    break;
                }
            }
        }

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ShowRestoreActivityAction action, CancellationToken ct)
    {
        currentState.Modals.Push(
            new ModalState(typeof(ConfirmModal), new Dictionary<string, object>
            {
                { nameof(ConfirmModal.Title), "Restore activity?" },
                {
                    nameof(ConfirmModal.Message),
                    "If you restore this activity you will be able to use it to register time again. All previously registered time will still be available. The activity can later be archived again."
                },
                { nameof(ConfirmModal.ConfirmText), "Restore" },
                {
                    nameof(ConfirmModal.OnConfirmAction), new ConfirmRestoreActivityAction(action.ActivityId)
                }
            }));
        return Task.FromResult(currentState);
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, ConfirmRestoreActivityAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var command = new RestoreActivityCommand(action.ActivityId);
        await commandHandler.ExecuteAsync(command, oc, ct);

        // TODO: Add projectId to the action to make this lookup faster
        foreach (var p in currentState.Projects)
        {
            foreach (var a in p.Activities)
            {
                if (Equals(a.Id, command.ActivityId))
                {
                    a.IsArchived = false;
                    break;
                }
            }
        }

        currentState.SafePopTopMostModal();
        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, LoadTimesheetAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var timesheetRepository = repositoryFactory.TimesheetRepository(oc.UserId);
        var timesheet = await timesheetRepository.GetOrNewTimesheetAsync(action.Date, ct);

        currentState.SelectedTimesheet = timesheet;
        currentState.SelectedDate = action.Date;

        return currentState;
    }

    private async Task<ApplicationState> HandleAsync(ApplicationState currentState, TimeAction action, CancellationToken ct)
    {
        var oc = await OperationContextForCurrentUser();
        var timesheetRepository = repositoryFactory.TimesheetRepository(oc.UserId);
        var timesheet = await timesheetRepository.GetOrNewTimesheetAsync(action.Date, ct);
        timesheet.Register(action.Slot);
        await timesheetRepository.UpdateByIdAsync(timesheet.Id, old => timesheet, ct);
        currentState.SelectedTimesheet = timesheet;

        return currentState;
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ExtendStartOfDay action, CancellationToken ct)
    {
        if (currentState.StartHourOfDay > 0)
            currentState.StartHourOfDay -= 1;
        return Task.FromResult(currentState);
    }

    private static Task<ApplicationState> HandleAsync(ApplicationState currentState, ExtendEndOfDay action, CancellationToken ct)
    {
        if (currentState.EndHourOfDay < 23)
            currentState.EndHourOfDay += 1;
        return Task.FromResult(currentState);
    }

    private async Task<OperationContext> OperationContextForCurrentUser()
    {
        var currentId = await userAuthorizationService.CurrentUserId();
        return new OperationContext(currentId, []);
    }

    private async Task<IDictionary<IdOf<Project>, IList<Activity>>> GetActivitiesPerProjectAsync(IdOf<User> userId, CancellationToken ct)
    {
        var activityRepository = repositoryFactory.ActivityRepository(userId);
        var activities = await activityRepository.GetAllAsync(ct)
            .ToListAsync(ct);
        var activitiesPerProject = activities.Aggregate(new Dictionary<IdOf<Project>, IList<Activity>>(),
            (acc, activity) =>
            {
                var pid = activity.ProjectId;
                if (!acc.ContainsKey(pid))
                    acc.Add(pid, new List<Activity>());
                acc[pid].Add(activity);
                return acc;
            });
        return activitiesPerProject;
    }

    private async Task<IList<Activity>> GetActivitiesForProjectAsync(IdOf<User> userId, IdOf<Project> projectId, CancellationToken ct)
    {
        // TODO: Add method to only read activities for this project action.ProjectId
        var all = await GetActivitiesPerProjectAsync(userId, ct);
        if (!all.TryGetValue(projectId, out var activities))
            activities = new List<Activity>();
        return activities;
    }

    private static ProjectViewModel FromProject(
        Project project,
        IDictionary<IdOf<Project>,
            IList<Activity>> activitiesPerProject,
        ProjectTotalUsage projectTotalUsage)
    {
        if (!activitiesPerProject.TryGetValue(project.Id, out var activities))
            activities = new List<Activity>();

        return new ProjectViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Updated = project.Updated ?? project.Created,
            Activities = activities.Select(a => FromActivity(a, projectTotalUsage)).ToList(),
            TotalMinutes = projectTotalUsage.TotalMinutes,
            LastUsed = projectTotalUsage.LastUsed,
            IsArchived = project.IsArchived,
        };
    }

    private static ActivityViewModel FromActivity(Activity activity, ProjectTotalUsage projectTotalUsage)
    {
        var activityUsage = projectTotalUsage.Activities.FirstOrDefault(a => a.ActivityId == activity.Id);
        return new ActivityViewModel
        {
            Id = activity.Id,
            ProjectId = activity.ProjectId,
            Name = activity.Name,
            Description = activity.Description,
            Color = activity.Color.ToHex(),
            DarkerColor = activity.Color.Darken(0.15).ToHex(),
            Updated = activity.Updated ?? activity.Created,
            TotalMinutes = activityUsage?.TotalMinutes ?? 0,
            IsArchived = activity.IsArchived,
        };
    }
}
