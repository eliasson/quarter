import domain/project
import domain/timesheet
import domain/user
import gleam/option
import model
import route.{type Route}
import rsvp

pub type Msg {
  Noop
  /// When a page navigation takes place.
  OnRouteChange(Route)

  /// Generic drop down menu that is keyed with an arbitrary key.
  OpenDropDownMenu(id: String)

  /// Opens the given dialog.
  OpenDialog(dialog: model.Dialog)

  ///When the modal backdrop is clicked, menu closed, etc.
  CloseModal

  /// When a dialog is confirmed (save, ok, etc).
  ConfirmDialog

  /// Dismiss an error notification.
  DismissError(id: String)

  /// Change the calendar to display timesheets for the next month.
  NextMonth

  /// Change the calendar to display timesheets for the previous month.
  PreviousMonth

  /// Change the timesheet view to display tomorrows timesheet.
  NextTimesheet

  /// Change the timesheet view to display yesterdays timesheet.
  PreviousTimesheet

  /// Selects an activity in the timesheet view's list of activities.
  /// None represents the "clear activity".
  SelectActivity(id: option.Option(project.ActivityId))

  /// Select / deselect a project in the mange project list.
  ToggleProject(id: project.ProjectId)

  /// Action that asks the user to confirm archive of the given activity.
  ConfirmArchiveActivity(activity: project.Activity)

  /// Archive the given activity.
  ArchiveActivity(activity: project.Activity)

  /// Action that asks the user to confirm deletion of the given activity.
  ConfirmDeleteActivity(activity: project.Activity)

  /// Delete the given activity.
  DeleteActivity(activity: project.Activity)

  /// Action that asks the user to confirm deletion of the given project.
  ConfirmDeleteProject(project: project.Project)

  /// Delete the given project.
  DeleteProject(project: project.Project)

  /// Action that asks the user to confirm archive of the given project.
  ConfirmArchiveProject(project: project.Project)

  /// Archive the given project.
  ArchiveProject(project: project.Project)

  //
  // Form messages
  //
  /// A text field was updated
  FormTextFieldUpdated(value: model.FormValue)

  //
  // Protocol messages
  //
  CurrentUserResult(Result(user.User, rsvp.Error))
  SystemUsersResult(Result(List(user.User), rsvp.Error))
  AddUserResult(Result(user.User, rsvp.Error))
  ProjectsResult(Result(List(project.Project), rsvp.Error))
  ArchiveActivityResult(Result(project.Activity, rsvp.Error))
  DeleteActivityResult(Result(project.Activity, rsvp.Error))
  DeleteProjectResult(Result(project.Project, rsvp.Error))
  ArchiveProjectResult(Result(project.Project, rsvp.Error))
  CreateProjectResult(Result(project.Project, rsvp.Error))
  UpdateProjectResult(Result(project.Project, rsvp.Error))
  CreateActivityResult(Result(project.Activity, rsvp.Error))
  UpdateActivityResult(Result(project.Activity, rsvp.Error))
  TimesheetsResult(Result(List(timesheet.Timesheet), rsvp.Error))
  TimesheetResult(Result(timesheet.Timesheet, rsvp.Error))
}
