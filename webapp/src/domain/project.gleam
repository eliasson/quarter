import domain/color
import gleam/dict
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

pub type ProjectCollection {
  ProjectCollection(
    projects: List(Project),
    by_project_id: dict.Dict(ProjectId, Project),
    by_activity_id: dict.Dict(ActivityId, Activity),
  )
}

pub fn empty() -> ProjectCollection {
  ProjectCollection(
    projects: [],
    by_project_id: dict.new(),
    by_activity_id: dict.new(),
  )
}

pub fn from_list(projects: List(Project)) -> ProjectCollection {
  let projects =
    sort_projects(projects)
    |> list.map(fn(p) { Project(..p, activities: sort_activities(p.activities)) })
  let by_project_id = dict.from_list(list.map(projects, fn(p) { #(p.id, p) }))
  let by_activity_id =
    list.fold(projects, dict.new(), fn(acc, p) {
      list.fold(p.activities, acc, fn(acc, a) { dict.insert(acc, a.id, a) })
    })
  ProjectCollection(projects:, by_project_id:, by_activity_id:)
}

pub fn to_list(c: ProjectCollection) -> List(Project) {
  c.projects
}

pub fn get_project(c: ProjectCollection, id: ProjectId) -> Result(Project, Nil) {
  dict.get(c.by_project_id, id)
}

pub fn get_activity(
  c: ProjectCollection,
  id: ActivityId,
) -> Result(Activity, Nil) {
  dict.get(c.by_activity_id, id)
}

pub fn put_project(c: ProjectCollection, p: Project) -> ProjectCollection {
  let updated =
    list.map(c.projects, fn(existing) {
      case existing.id == p.id {
        True -> Project(..p, activities: existing.activities)
        False -> existing
      }
    })
  from_list(updated)
}

pub fn put_activity(c: ProjectCollection, a: Activity) -> ProjectCollection {
  let updated =
    list.map(c.projects, fn(p) {
      case p.id == a.project_id {
        True -> {
          let activities =
            list.map(p.activities, fn(existing) {
              case existing.id == a.id {
                True -> a
                False -> existing
              }
            })
          Project(..p, activities:)
        }
        False -> p
      }
    })
  from_list(updated)
}

pub fn remove_project(c: ProjectCollection, id: ProjectId) -> ProjectCollection {
  from_list(list.filter(c.projects, fn(p) { p.id != id }))
}

pub fn remove_activity(
  c: ProjectCollection,
  id: ActivityId,
) -> ProjectCollection {
  let updated =
    list.map(c.projects, fn(p) {
      let activities = list.filter(p.activities, fn(a) { a.id != id })
      Project(..p, activities:)
    })
  from_list(updated)
}

/// Return only non-archived projects, sorted alphabetically.
pub fn active_projects(subject: ProjectCollection) -> List(Project) {
  subject.projects
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
