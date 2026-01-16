import gleeunit/should
import project
import protocol
import test_util.{arbitrary_activity, arbitrary_project}

pub fn activity_url_test() {
  let project_id = project.ProjectId("P-001")
  let id = project.ActivityId("A-001")

  project.Activity(..arbitrary_activity(), project_id:, id:)
  |> protocol.activity_url
  |> should.equal("/api/projects/P-001/activities/A-001")
}

pub fn project_url_test() {
  let id = project.ProjectId("P-001")

  project.Project(..arbitrary_project(), id:)
  |> protocol.project_url
  |> should.equal("/api/projects/P-001")
}
