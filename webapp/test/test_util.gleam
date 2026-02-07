import domain/color
import domain/duration
import domain/project
import domain/timesheet
import gleam/list
import gleam/option
import gleam/result
import gleam/time/timestamp
import model
import util/timestamp as tsutil

/// Get the first element from a tuple
pub fn first(t: #(a, b)) {
  t.0
}

pub fn arbitrary_project() -> project.Project {
  project.Project(
    project.ProjectId("P01"),
    "Project Alpha",
    "The Alpha project",
    False,
    tsutil.timestamp_zero(),
    option.None,
    [],
  )
}

pub fn arbitrary_activity() -> project.Activity {
  project.Activity(
    project.ActivityId("A01"),
    project.ProjectId("P01"),
    "Activity Alpha",
    "",
    color.Color(142, 135, 245),
    False,
    tsutil.timestamp_zero(),
    option.None,
  )
}

pub fn m_project_by_id(
  model: model.Model,
  id: project.ProjectId,
) -> Result(project.Project, Nil) {
  project_by_id(model.projects, id)
}

pub fn project_by_id(
  projects: List(project.Project),
  id: project.ProjectId,
) -> Result(project.Project, Nil) {
  list.find(projects, fn(p) { p.id == id })
}

pub fn m_activity_by_id(
  model: model.Model,
  id: project.ActivityId,
) -> Result(project.Activity, Nil) {
  activity_by_id(model.projects, id)
}

pub fn activity_by_id(
  projects: List(project.Project),
  id: project.ActivityId,
) -> Result(project.Activity, Nil) {
  let activities =
    list.flat_map(projects, fn(p) {
      list.filter(p.activities, fn(a) { a.id == id })
    })

  case activities {
    [a] -> Ok(a)
    _ -> Error(Nil)
  }
}

pub fn projects(m: model.Model) -> List(project.Project) {
  m.projects
}

pub fn activities(p: project.Project) -> List(project.Activity) {
  p.activities
}

pub fn get_dialog_value(
  m: model.Model,
  field_id: String,
) -> option.Option(String) {
  // Get the top most dialog and see if the state contains the given field.
  let value = case list.last(m.dialogs) {
    Ok(d) -> {
      case d {
        model.AddUserDialog(state) -> {
          case field_id {
            "email" -> option.Some(state.email.value.value)
            _ -> option.None
          }
        }

        model.AddProjectDialog(state) -> {
          case field_id {
            "name" -> option.Some(state.name.value)
            "description" -> option.Some(state.description.value)
            _ -> option.None
          }
        }

        model.EditProjectDialog(state, _) -> {
          case field_id {
            "name" -> option.Some(state.name.value)
            _ -> option.None
          }
        }

        model.AddActivityDialog(state, _) -> {
          case field_id {
            "name" -> option.Some(state.name.value)
            "description" -> option.Some(state.description.value)
            "color" -> option.Some(color.to_hex(state.color.value))
            _ -> option.None
          }
        }

        model.EditActivityDialog(state, _) -> {
          case field_id {
            "name" -> option.Some(state.name.value)
            "description" -> option.Some(state.description.value)
            "color" -> option.Some(color.to_hex(state.color.value))
            _ -> option.None
          }
        }

        _ -> {
          option.None
        }
      }
    }
    _ -> option.None
  }
  value
}

pub fn new_timesheet(date: String) -> Result(timesheet.Timesheet, Nil) {
  // TODO Add logic to create a timesheet with slots when needed by tests.
  timestamp.parse_rfc3339(date)
  |> result.map(fn(date) { timesheet.Timesheet(date, duration.Minutes(0), []) })
}
