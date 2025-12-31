import gleeunit/should
import project
import protocol
import test_util.{arbitrary_activity}

pub fn activity_url_test() {
  let project_id = project.ProjectId("P-001")
  let id = project.ActivityId("A-001")

  project.Activity(..arbitrary_activity(), project_id:, id:)
  |> protocol.activity_url
  |> should.equal("/api/projects/P-001/activities/A-001")
}
