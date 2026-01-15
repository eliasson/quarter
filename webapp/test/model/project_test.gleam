import gleam/list
import gleeunit/should
import model
import project
import test_util.{arbitrary_project}

pub fn delete_project_test() {
  let project_id = project.ProjectId("TheProject")

  let projects = [
    project.Project(..arbitrary_project(), name: "One"),
    project.Project(..arbitrary_project(), name: "Two", id: project_id),
    project.Project(..arbitrary_project(), name: "Three"),
  ]

  model.Model(..model.initial_model(), projects:)
  |> model.delete_project(project_id)
  |> test_util.projects
  |> list.map(fn(p) { p.name })
  |> should.equal(["One", "Three"])
}
