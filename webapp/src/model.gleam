import dialogs/activity_dialog
import dialogs/project_dialog
import dialogs/user_dialog
import domain/project
import domain/user
import gleam/list
import gleam/set
import gleam/time/timestamp
import route
import seq
import types

pub type Model {
  Model(
    is_authenticated: Bool,
    route: route.Route,
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
    projects: List(project.Project),
    /// The list of ID for the projects that are expanded in the manage view.
    expanded_projects: set.Set(project.ProjectId),
  )
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
    today: timestamp.system_time(),
    dropdowns: [],
    dialogs: [],
    users: [],
    errors: [],
    projects: [],
    expanded_projects: set.new(),
  )
}

pub fn navigate_to(m: Model, route: route.Route) -> Model {
  Model(..m, route: route)
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
  let projects =
    list.map(m.projects, fn(p) {
      case p.id {
        id if activity.project_id == id -> {
          let activities =
            list.map(p.activities, fn(a) {
              case a.id {
                aid if aid == activity.id -> activity
                _ -> a
              }
            })
          project.Project(..p, activities:)
        }
        _ -> p
      }
    })

  Model(..m, projects:)
}

pub fn update_project(m: Model, project: project.Project) -> Model {
  let projects =
    list.map(m.projects, fn(p) {
      case p.id {
        id if project.id == id ->
          project.Project(..project, activities: p.activities)
        _ -> p
      }
    })

  Model(..m, projects:)
}

pub fn delete_activity(
  m: Model,
  project_id: project.ProjectId,
  activity_id: project.ActivityId,
) -> Model {
  let projects =
    list.map(m.projects, fn(p) {
      case p.id {
        id if project_id == id -> {
          let activities =
            list.filter(p.activities, fn(a) { a.id != activity_id })
          project.Project(..p, activities:)
        }
        _ -> p
      }
    })

  Model(..m, projects:)
}

pub fn delete_project(m: Model, project_id: project.ProjectId) -> Model {
  let projects = list.filter(m.projects, fn(p) { p.id != project_id })
  Model(..m, projects:)
}
