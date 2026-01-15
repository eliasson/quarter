import gleam/list
import gleam/option
import model
import project
import util

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
    util.timestamp_zero(),
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
    util.Color(142, 135, 245),
    False,
    util.timestamp_zero(),
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

pub fn activities(p: project.Project) -> List(project.Activity) {
  p.activities
}
