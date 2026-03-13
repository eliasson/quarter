import domain/project
import gleam/list
import gleeunit/should
import model
import test_util.{arbitrary_activity, arbitrary_project}

pub fn active_projects_excludes_archived_test() {
  let projects = [
    project.Project(..arbitrary_project(), name: "Active B"),
    project.Project(..arbitrary_project(), name: "Archived", is_archived: True),
    project.Project(..arbitrary_project(), name: "Active A"),
  ]

  project.active_projects(projects)
  |> list.map(fn(p) { p.name })
  |> should.equal(["Active A", "Active B"])
}

pub fn active_activities_excludes_archived_test() {
  let activities = [
    project.Activity(..arbitrary_activity(), name: "Active B"),
    project.Activity(
      ..arbitrary_activity(),
      name: "Archived",
      is_archived: True,
    ),
    project.Activity(..arbitrary_activity(), name: "Active A"),
  ]

  project.active_activities(activities)
  |> list.map(fn(a) { a.name })
  |> should.equal(["Active A", "Active B"])
}

pub fn sort_projects_alphabetically_test() {
  let projects = [
    project.Project(..arbitrary_project(), name: "Zebra"),
    project.Project(..arbitrary_project(), name: "Apple"),
    project.Project(..arbitrary_project(), name: "Mango"),
  ]

  project.sort_projects(projects)
  |> list.map(fn(p) { p.name })
  |> should.equal(["Apple", "Mango", "Zebra"])
}

pub fn sort_projects_archived_last_test() {
  let projects = [
    project.Project(..arbitrary_project(), name: "Active B"),
    project.Project(
      ..arbitrary_project(),
      name: "Archived A",
      is_archived: True,
    ),
    project.Project(..arbitrary_project(), name: "Active A"),
    project.Project(
      ..arbitrary_project(),
      name: "Archived B",
      is_archived: True,
    ),
  ]

  project.sort_projects(projects)
  |> list.map(fn(p) { p.name })
  |> should.equal(["Active A", "Active B", "Archived A", "Archived B"])
}

pub fn sort_activities_alphabetically_test() {
  let activities = [
    project.Activity(..arbitrary_activity(), name: "Zebra"),
    project.Activity(..arbitrary_activity(), name: "Apple"),
    project.Activity(..arbitrary_activity(), name: "Mango"),
  ]

  project.sort_activities(activities)
  |> list.map(fn(a) { a.name })
  |> should.equal(["Apple", "Mango", "Zebra"])
}

pub fn sort_activities_archived_last_test() {
  let activities = [
    project.Activity(..arbitrary_activity(), name: "Active B"),
    project.Activity(
      ..arbitrary_activity(),
      name: "Archived A",
      is_archived: True,
    ),
    project.Activity(..arbitrary_activity(), name: "Active A"),
    project.Activity(
      ..arbitrary_activity(),
      name: "Archived B",
      is_archived: True,
    ),
  ]

  project.sort_activities(activities)
  |> list.map(fn(a) { a.name })
  |> should.equal(["Active A", "Active B", "Archived A", "Archived B"])
}

pub fn delete_project_test() {
  let project_id = project.ProjectId("TheProject")

  let projects = [
    project.Project(..arbitrary_project(), name: "One"),
    project.Project(..arbitrary_project(), name: "Two", id: project_id),
    project.Project(..arbitrary_project(), name: "Three"),
  ]

  model.Model(..model.initial_model(), projects: project.from_list(projects))
  |> model.delete_project(project_id)
  |> test_util.projects
  |> list.map(fn(p) { p.name })
  |> should.equal(["One", "Three"])
}
