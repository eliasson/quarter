import domain/color
import gleam/list
import gleam/option
import gleam/order
import gleam/string
import gleam/time/timestamp.{type Timestamp}

pub type ProjectId {
  ProjectId(value: String)
}

pub type ActivityId {
  ActivityId(value: String)
}

pub type Project {
  Project(
    id: ProjectId,
    name: String,
    description: String,
    is_archived: Bool,
    created: Timestamp,
    updated: option.Option(Timestamp),
    activities: List(Activity),
  )
}

pub type Activity {
  Activity(
    id: ActivityId,
    project_id: ProjectId,
    name: String,
    description: String,
    color: color.Color,
    is_archived: Bool,
    created: Timestamp,
    updated: option.Option(Timestamp),
  )
}

/// Return only non-archived projects, sorted alphabetically.
pub fn active_projects(subject: List(Project)) -> List(Project) {
  subject
  |> list.filter(fn(p) { !p.is_archived })
  |> list.sort(fn(a, b) { string.compare(a.name, b.name) })
}

/// Return only non-archived activities, sorted alphabetically.
pub fn active_activities(subject: List(Activity)) -> List(Activity) {
  subject
  |> list.filter(fn(a) { !a.is_archived })
  |> list.sort(fn(a, b) { string.compare(a.name, b.name) })
}

/// Sort the given list of projects alphabetically, but with the archived projects last (each section
/// of archived / not archived will be sorted alphabetically).
pub fn sort_projects(subject: List(Project)) {
  list.sort(subject, fn(a, b) {
    case a.is_archived, b.is_archived {
      True, False -> order.Gt
      False, True -> order.Lt
      _, _ -> string.compare(a.name, b.name)
    }
  })
}

/// Sort the given list of activities alphabetically, but with the archived activity last (each section
/// of archived / not archived will be sorted alphabetically).
pub fn sort_activities(subject: List(Activity)) {
  list.sort(subject, fn(a, b) {
    case a.is_archived, b.is_archived {
      True, False -> order.Gt
      False, True -> order.Lt
      _, _ -> string.compare(a.name, b.name)
    }
  })
}
