import dialogs/activity_dialog
import dialogs/project_dialog
import dialogs/user_dialog
import domain/project
import domain/timesheet
import domain/user
import gleam/list
import gleam/option
import gleam/set
import gleam/time/timestamp
import i18n
import route
import types
import util/seq
import util/timestamp as tsutil

pub type Model {
  Model(
    is_authenticated: Bool,
    route: route.Route,
    /// The users language preference.
    lang: i18n.Language,
    /// Our understanding of todays date, based on the users system time. Set during certain
    /// navigation events. I.e. it is always older than the current time.
    today: timestamp.Timestamp,
    /// The open drop down menus, a list to allow for nested menus.
    dropdowns: List(DropDownMenu),
    /// The open dialogs as a reverse stack (top most is last in list).
    dialogs: List(Dialog),
    /// The list of all system users if loaded (for admin).
    users: List(user.User),
    /// The errors that have occured and that are not dismissed
    errors: List(ApplicationError),
    /// The projects available to the user (regardless of archived state).
    projects: project.ProjectCollection,
    /// The list of ID for the projects that are expanded in the manage view.
    expanded_projects: set.Set(project.ProjectId),
    /// The current months timesheets.
    timesheets: List(timesheet.Timesheet),
    /// The selected activity. None represents the "clear activity" in the timesheet view.
    selected_activity: option.Option(project.ActivityId),
    /// The start of day hour used when rendering a timesheet. This time is inclusive.
    start_of_day: Int,
    /// The end of day hour used when rendering a timesheet. This time is inclusive.
    end_of_day: Int,
    /// The active timesheet being viewed or edited.
    active_timesheet: option.Option(timesheet.Timesheet),
    /// When there is an active registration (user have painted cells with an activity or clear).
    active_registration: option.Option(ActiveRegistration),
  )
}

/// The index of a specific quarter for a day (0 - 95)
pub type QuarterIndex =
  Int

pub type ActiveRegistration {
  /// The ongoing registration
  /// - start - The quarter index where the user first started the registration.
  /// - end - The current quarter index where the registration should end. This can either be before or after the start!
  ActiveRegistration(start: QuarterIndex, end: QuarterIndex)
}

pub type DropDownMenu {
  DropDownMenu(id: String)
}

pub type Dialog {
  AddUserDialog(state: user_dialog.State)
  AddProjectDialog(state: project_dialog.State)
  EditProjectDialog(state: project_dialog.State, project: project.Project)
  AddActivityDialog(state: activity_dialog.State, project: project.Project)
  EditActivityDialog(state: activity_dialog.State, activity: project.Activity)
  ArchiveActivityDialog(activity: project.Activity)
  DeleteActivityDialog(activity: project.Activity)
  DeleteProjectDialog(project: project.Project)
  ArchiveProjectDialog(project: project.Project)
}

/// An application error is to be displayed for the user when something
/// goes wrong. These are currently not actionable, just intended as an FYI.
pub type ApplicationError {
  ApplicationError(id: String, message: String)
}

/// A value emitted from a forms input control. E.g. when the user enters text in a <input> element.
pub type FormValue =
  types.FormValue

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(
    is_authenticated: False,
    route: route.Home,
    lang: i18n.English,
    today: timestamp.system_time(),
    dropdowns: [],
    dialogs: [],
    users: [],
    errors: [],
    projects: project.empty(),
    expanded_projects: set.new(),
    timesheets: [],
    selected_activity: option.None,
    start_of_day: 6,
    end_of_day: 18,
    active_timesheet: option.None,
    active_registration: option.None,
  )
}

pub fn go_to_next_month(m: Model) -> Model {
  Model(..m, today: tsutil.next_first_of_month(m.today))
}

pub fn go_to_previous_month(m: Model) -> Model {
  Model(..m, today: tsutil.previous_first_of_month(m.today))
}

pub fn go_to_yesterday(m: Model) -> Model {
  Model(..m, today: tsutil.yesterday(m.today))
}

pub fn go_to_tomorrow(m: Model) -> Model {
  Model(..m, today: tsutil.tomorrow(m.today))
}

pub fn navigate_to(m: Model, route: route.Route) -> Model {
  let updated_model = case route {
    route.Home -> Model(..m, today: timestamp.system_time())
    route.Timesheet(ts) -> Model(..m, today: ts)
    _ -> m
  }

  Model(..updated_model, route: route)
}

pub fn open_drop_down_menu(m: Model, id: String) -> Model {
  Model(..m, dropdowns: list.append(m.dropdowns, [DropDownMenu(id)]))
}

pub fn is_drop_down_menu_open(m: Model, id: String) -> Bool {
  case list.find(m.dropdowns, fn(dd) { dd.id == id }) {
    Ok(_) -> True
    _ -> False
  }
}

/// Close the top most modal (e.g. drop-down menu or dialog).
pub fn close_modal(m: Model) {
  // TODO: How to find the top most?
  let dropdowns = seq.drop_last(m.dropdowns)
  let dialogs = seq.drop_last(m.dialogs)

  Model(..m, dropdowns: dropdowns, dialogs: dialogs)
}

/// Close all modals (e.g. drop-down menus and dialogs).
pub fn close_all_modals(m: Model) {
  Model(..m, dropdowns: [], dialogs: [])
}

pub fn set_current_user(m: Model, _user: user.User) {
  Model(..m, is_authenticated: True)
}

pub fn set_users(m: Model, users: List(user.User)) {
  Model(..m, users: users)
}

/// Update the model with the timesheets for the current month.
pub fn set_timesheets(m: Model, timesheets: List(timesheet.Timesheet)) {
  Model(..m, timesheets: timesheets)
}

pub fn open_dialog(m: Model, dialog: Dialog) -> Model {
  Model(..m, dropdowns: [], dialogs: list.append(m.dialogs, [dialog]))
}

pub fn add_error(m: Model, error: ApplicationError) {
  Model(..m, errors: list.append(m.errors, [error]))
}

pub fn dismiss_error(m: Model, id: String) {
  let errors = list.filter(m.errors, fn(e) { e.id != id })
  Model(..m, errors:)
}

pub fn current_dialog(m: Model) -> Result(Dialog, Nil) {
  list.last(m.dialogs)
}

pub fn toggle_project(m: Model, id: project.ProjectId) {
  let expanded_projects = case set.contains(m.expanded_projects, id) {
    True -> set.delete(m.expanded_projects, id)
    False -> set.insert(m.expanded_projects, id)
  }

  Model(..m, expanded_projects:)
}

pub fn is_project_expanded(m: Model, id: project.ProjectId) -> Bool {
  set.contains(m.expanded_projects, id)
}

pub fn update_dialog_value(m: Model, value: FormValue) -> Model {
  let updated_dialogs = case list.last(m.dialogs) {
    Ok(d) -> {
      case d {
        AddUserDialog(state) -> [
          AddUserDialog(state: user_dialog.update(state, value)),
        ]

        AddProjectDialog(state) -> [
          AddProjectDialog(state: project_dialog.update(state, value)),
        ]

        EditProjectDialog(state, project) -> [
          EditProjectDialog(project_dialog.update(state, value), project),
        ]

        AddActivityDialog(state, project) -> [
          AddActivityDialog(activity_dialog.update(state, value), project),
        ]

        EditActivityDialog(state, activity) -> [
          EditActivityDialog(activity_dialog.update(state, value), activity),
        ]

        _ -> [d]
      }
    }
    _ -> []
  }

  let dialogs =
    seq.drop_last(m.dialogs)
    |> list.append(updated_dialogs)

  Model(..m, dialogs:)
}

pub fn new_user_dialog() {
  AddUserDialog(user_dialog.new())
}

pub fn new_project_dialog() {
  AddProjectDialog(project_dialog.new())
}

pub fn edit_project_dialog(project: project.Project) {
  EditProjectDialog(project_dialog.edit(project), project)
}

pub fn new_activity_dialog(project: project.Project) {
  AddActivityDialog(activity_dialog.new(), project)
}

pub fn edit_activity_dialog(activity: project.Activity) {
  EditActivityDialog(activity_dialog.edit(activity), activity)
}

/// Replace the current activity with the given one. Used after successful activity modifications.
pub fn update_activity(m: Model, activity: project.Activity) -> Model {
  Model(..m, projects: project.put_activity(m.projects, activity))
}

pub fn update_project(m: Model, p: project.Project) -> Model {
  Model(..m, projects: project.put_project(m.projects, p))
}

pub fn delete_activity(m: Model, activity_id: project.ActivityId) -> Model {
  Model(..m, projects: project.remove_activity(m.projects, activity_id))
}

pub fn delete_project(m: Model, project_id: project.ProjectId) -> Model {
  Model(..m, projects: project.remove_project(m.projects, project_id))
}

import gleam/int

pub fn extend_start_of_day(m: Model) {
  let start_of_day = int.max(0, m.start_of_day - 1)
  Model(..m, start_of_day:)
}

pub fn extend_end_of_day(m: Model) {
  let end_of_day = int.min(23, m.end_of_day + 1)
  Model(..m, end_of_day:)
}

pub fn set_active_timesheet(m: Model, timesheet: timesheet.Timesheet) -> Model {
  Model(..m, active_timesheet: option.Some(timesheet))
}

/// Called when a quarter is selected in the timesheet grid.
/// The first selection will setup an active registration, subsequent selections will extend it.
pub fn select_quarter(m: Model, index: QuarterIndex) -> Model {
  let registration = case m.active_registration {
    option.Some(reg) -> ActiveRegistration(..reg, end: index)
    option.None -> ActiveRegistration(index, index)
  }

  Model(..m, active_registration: option.Some(registration))
}

/// Clears any active registration.
pub fn clear_registration(m: Model) -> Model {
  Model(..m, active_registration: option.None)
}
