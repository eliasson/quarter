using Quarter.Core.Models;
using Quarter.Core.UI.State;
using Quarter.Core.Utils;
using Quarter.State.Forms;

namespace Quarter.State;

/// <summary>
/// Close the top-most modal (if multiple modals are showed).
/// </summary>
public record CloseModalAction() : IAction;

#region User related

/// <summary>
/// Show the add new user modal.
/// </summary>
public record ShowAddUserAction() : IAction;

/// <summary>
/// Show the remove user confirmation modal.
/// </summary>
/// <param name="UserId">The string representation of the user's Id</param>
public record ShowRemoveUserAction(string UserId) : IAction;

/// <summary>
/// Confirm to remove a user.
/// </summary>
/// <param name="UserId">The string representation of the user's Id</param>
public record ConfirmRemoveUserAction(string UserId) : IAction;

/// <summary>
/// Add a new user based on the given form data.
/// </summary>
/// <param name="FormData">The data populated from the user form</param>
public record AddUserAction(UserFormData FormData) : IAction;

#endregion

#region Project related

/// <summary>
/// Load all projects and activities onto the application state.
/// </summary>
/// <param name="Force"></param>
public record LoadProjects(bool Force = false) : IAction;

/// <summary>
/// Show the add project modal.
/// </summary>
public record ShowAddProjectAction() : IAction;

/// <summary>
/// Add a new project.
/// </summary>
/// <param name="FormData">The data populated from the project form</param>
public record AddProjectAction(ProjectFormData FormData) : IAction;

/// <summary>
/// Show a remove project confirmation modal.
/// </summary>
/// <param name="ProjectId">The ID of the project to remove</param>
public record ShowRemoveProjectAction(IdOf<Project> ProjectId) : IAction;

/// <summary>
/// Confirm and remove the given project.
/// </summary>
/// <param name="ProjectId">The ID of the project to remove</param>
public record ConfirmRemoveProjectAction(IdOf<Project> ProjectId) : IAction;

/// <summary>
/// Show the edit project modal.
/// </summary>
/// <param name="ProjectId">The ID of the project to edit</param>
public record ShowEditProjectAction(IdOf<Project> ProjectId) : IAction;

/// <summary>
/// Update the given project with data from the project modal.
/// </summary>
/// <param name="ProjectId">The ID of the project to edit</param>
/// <param name="FormData">The data populated from the project form</param>
public record EditProjectAction(IdOf<Project> ProjectId, ProjectFormData FormData) : IAction;

/// <summary>
/// Show archive project confirmation modal.
/// </summary>
/// <param name="ProjectId">The ID of the project to archive</param>
public record ShowArchiveProjectAction(IdOf<Project> ProjectId) : IAction;

/// <summary>
/// Confirm and archive the project.
/// </summary>
/// <param name="ProjectId">The ID of the project to archive</param>
public record ConfirmArchiveProjectAction(IdOf<Project> ProjectId) : IAction;

/// <summary>
/// Show restore project confirmation modal.
/// </summary>
/// <param name="ProjectId">The ID of the project to restore</param>
public record ShowRestoreProjectAction(IdOf<Project> ProjectId) : IAction;

/// <summary>
/// Confirm and restore the project.
/// </summary>
/// <param name="ProjectId">The ID of the project to restore</param>
public record ConfirmRestoreProjectAction(IdOf<Project> ProjectId) : IAction;

#endregion

#region Activity related

/// <summary>
/// Show the add activity modal.
/// </summary>
/// <param name="ProjectId">The ID of the project the activity should be associated with</param>
public record ShowAddActivityAction(IdOf<Project> ProjectId) : IAction;

/// <summary>
/// Add a new activity associated with the given project.
/// </summary>
/// <param name="ProjectId">The ID of the project the activity should be associated with</param>
/// <param name="FormData">The data populated from the project form</param>
public record AddActivityAction(IdOf<Project> ProjectId, ActivityFormData FormData) : IAction;

/// <summary>
/// Show a remove activity confirmation modal.
/// </summary>
/// <param name="ActivityId">The ID of the activity to remove</param>
public record ShowRemoveActivityAction(IdOf<Activity> ActivityId) : IAction;

/// <summary>
/// Confirm and remove the given activity.
/// </summary>
/// <param name="ActivityId">The ID of the activity to remove</param>
public record ConfirmRemoveActivityAction(IdOf<Activity> ActivityId) : IAction;

/// <summary>
/// Show the edit activity modal.
/// </summary>
/// <param name="ProjectId">The ID of the project the activity is associated with</param>
/// <param name="ActivityId">The ID of the activity to edit</param>
public record ShowEditActivityAction(IdOf<Project> ProjectId, IdOf<Activity> ActivityId) : IAction;

/// <summary>
/// Update the given activity with data from the activity modal.
/// </summary>
/// <param name="ProjectId">The ID of the project the activity is associated with</param>
/// <param name="ActivityId">The ID of the activity to edit</param>
/// <param name="FormData">The data populated from the activity form</param>
public record EditActivityAction(IdOf<Project> ProjectId, IdOf<Activity> ActivityId, ActivityFormData FormData) : IAction;

/// <summary>
/// Show archived activity confirmation modal.
/// </summary>
/// <param name="ActivityId">The ID of the activity to archive</param>
public record ShowArchiveActivityAction(IdOf<Activity> ActivityId) : IAction;

/// <summary>
/// Confirm and archive the activity.
/// </summary>
/// <param name="ActivityId">The ID of the activity to archive</param>
public record ConfirmArchiveActivityAction(IdOf<Activity> ActivityId) : IAction;

/// <summary>
/// Show restore activity confirmation modal.
/// </summary>
/// <param name="ActivityId">The ID of the activity to restore</param>
public record ShowRestoreActivityAction(IdOf<Activity> ActivityId) : IAction;

/// <summary>
/// Confirm and restore the activity.
/// </summary>
/// <param name="ActivityId">The ID of the activity to restore</param>
public record ConfirmRestoreActivityAction(IdOf<Activity> ActivityId) : IAction;

#endregion

#region Timesheet related

/// <summary>
/// Load the timesheet for the give date and store in the application state.
/// </summary>
/// <param name="Date">The date for the timesheet</param>
public record LoadTimesheetAction(Date Date) : IAction;

/// <summary>
/// Set the currently selected activity on the application state.
/// </summary>
/// <param name="SelectedActivity">The selected activity</param>
public record SelectActivityAction(SelectedActivity SelectedActivity) : IAction;

/// <summary>
/// Unset any previously selected activity from the application state.
/// </summary>
public record SelectEraseActivityAction() : IAction;

public abstract record TimeAction(Date Date, TimeSlot Slot) : IAction;

/// <summary>
/// Register a time slot for the timesheet with the given date.
/// </summary>
/// <param name="Date">The date of the timesheet</param>
/// <param name="Slot">The activity time slot to register</param>
public record RegisterTimeAction(Date Date, TimeSlot Slot) : TimeAction(Date, Slot);

/// <summary>
/// Erase any previously registered time slot from the timesheet with the given date.
/// </summary>
/// <param name="Date">The date of the timesheet</param>
/// <param name="Slot">The time slot to erase</param>
public record EraseTimeAction(Date Date, TimeSlot Slot) : TimeAction(Date, Slot);

/// <summary>
/// Extending the start of day with one hour, which is to move the start of day one hour earlier (if possible).
/// </summary>
public record ExtendStartOfDay : IAction;

/// <summary>
/// Extending the end of day with one hour, which is to move the end of day one hour later (if possible).
/// </summary>
public record ExtendEndOfDay : IAction;

#endregion
