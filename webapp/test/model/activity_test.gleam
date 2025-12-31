import gleeunit/should
import model
import project
import test_util.{arbitrary_activity, arbitrary_project}

pub fn update_activity_test() {
  let activity =
    project.Activity(
      ..arbitrary_activity(),
      project_id: project.ProjectId("TheProject"),
      id: project.ActivityId("123"),
      name: "The Activity",
    )

  let projects = [
    project.Project(..arbitrary_project(), activities: [arbitrary_activity()]),
    project.Project(
      ..arbitrary_project(),
      id: project.ProjectId("TheProject"),
      activities: [activity],
    ),
  ]

  let updated_activity =
    model.Model(..model.initial_model(), projects:)
    |> model.update_activity(
      project.Activity(..activity, name: "Updated Activity"),
    )
    |> test_util.m_activity_by_id(project.ActivityId("123"))
    |> should.be_ok

  should.equal(updated_activity.name, "Updated Activity")
}
