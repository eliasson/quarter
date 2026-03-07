import domain/duration
import domain/project
import domain/timesheet
import gleeunit/should
import test_util

pub fn it_should_be_empty_test() {
  let sheet =
    test_util.new_timesheet("2026-02-07T18:00:00Z", [])
    |> should.be_ok

  sheet.slots |> should.equal([])
  sheet.duration |> should.equal(duration.Minutes(0))
}

pub fn it_should_have_emtpy_summary_for_timesheet_test() {
  test_util.new_timesheet("2026-02-07T18:00:00Z", [])
  |> should.be_ok
  |> timesheet.summary([])
  |> should.equal(timesheet.TimesheetSummary(duration.Minutes(0), []))
}

pub fn it_should_have_expected_summary_for_timesheet_test() {
  let project_one =
    project.Project(
      ..test_util.arbitrary_project(),
      id: project.ProjectId("P1"),
      name: "Project One",
    )

  let project_two =
    project.Project(
      ..test_util.arbitrary_project(),
      id: project.ProjectId("P2"),
      name: "Project Two",
    )

  let activity_one_a =
    project.Activity(
      ..test_util.arbitrary_activity(),
      id: project.ActivityId("P1 A"),
      name: "P1 Alpha",
      project_id: project_one.id,
    )

  let activity_one_b =
    project.Activity(
      ..test_util.arbitrary_activity(),
      id: project.ActivityId("P1 B"),
      name: "P1 Bravo",
      project_id: project_one.id,
    )

  let activity_two_a =
    project.Activity(
      ..test_util.arbitrary_activity(),
      id: project.ActivityId("P2 A"),
      name: "P2 Alpha",
      project_id: project_two.id,
    )

  let projects = [
    project.Project(..project_one, activities: [activity_one_a, activity_one_b]),
    project.Project(..project_two, activities: [activity_two_a]),
  ]

  let slots = [
    timesheet.TimeSlot(project_one.id, activity_one_a.id, 0, 3),
    timesheet.TimeSlot(project_two.id, activity_two_a.id, 6, 4),
    timesheet.TimeSlot(project_one.id, activity_one_b.id, 12, 6),
    timesheet.TimeSlot(project_two.id, activity_two_a.id, 20, 4),
  ]

  test_util.new_timesheet("2026-02-07T18:00:00Z", slots)
  |> should.be_ok
  |> timesheet.summary(projects)
  |> should.equal(
    timesheet.TimesheetSummary(duration.Minutes(45 + 60 + 90 + 60), [
      timesheet.ProjectDetail(project_one.name, [
        timesheet.ActivityDetail(activity_one_b.name, duration.Minutes(90)),
        timesheet.ActivityDetail(activity_one_a.name, duration.Minutes(45)),
      ]),
      timesheet.ProjectDetail(project_two.name, [
        timesheet.ActivityDetail(activity_two_a.name, duration.Minutes(120)),
      ]),
    ]),
  )
}
