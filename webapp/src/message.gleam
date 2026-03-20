import domain/project
import domain/report
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
  /// Logout the current user by navigating to the logout URL.
  Logout

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
  SelectActivity(id: option.Option(project.Activity))

  /// Extend the start of day to one hour earlier.
  ExtendStartOfDay

  // Extend the day to one hour later.
  ExtendEndOfDay

  /// Called to start registering time by mouse down in the timesheet grid
  StartRegistering(index: Int)

  /// Called when highlighting (mouse over) a quarter whilst being in registering mode.
  UpdateRegistering(index: Int)

  /// Called when releasing the mouse down in the timesheet grid. Commits the time registration.
  CommitRegistering

  /// Used from the timesheet to open/close the activity picker.
  ToggleActivityPicker

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

  // Report view ---------------------------------------------------------------------------------
  //
  /// Change to the report for the next week (relative to the current report's date).
  NextReportWeek

  /// Change to the report for the previous week (relative to the current report's date).
  PreviousReportWeek

  // Form messages ------------------------------------------------------------------------------
  //
  /// A text field was updated
  FormTextFieldUpdated(value: model.FormValue)

  // Protocol messages ---------------------------------------------------------------------------
  //
  CurrentUserResult(Result(user.User, rsvp.Error))
  SystemUsersResult(Result(List(user.User), rsvp.Error))
  AddUserResult(Result(user.User, rsvp.Error))
  ProjectsResult(Result(project.ProjectCollection, rsvp.Error))
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
  RegisterTimeResult(Result(timesheet.Timesheet, rsvp.Error))
  ReportResult(Result(report.WeeklyReport, rsvp.Error))
}
