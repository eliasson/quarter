import domain/duration
import domain/project
import domain/timesheet
import gleam/json
import gleam/list
import gleeunit/should
import protocol

pub fn decode_minimal_timesheet_test() {
  let json_string =
    "{\"date\":\"2026-02-28\",\"totalMinutes\":120,\"timeSlots\":[]}"

  let result =
    json.parse(json_string, protocol.timesheet_decoder())
    |> should.be_ok

  result.duration |> should.equal(duration.Minutes(120))
  result.slots |> should.equal([])
}

pub fn decode_timesheets_response_test() {
  let json_string =
    "{\"timesheets\":[{\"date\":\"2026-02-28\",\"totalMinutes\":0,\"timeSlots\":[]},{\"date\":\"2026-02-27\",\"totalMinutes\":240,\"timeSlots\":[]}]}"

  let result =
    json.parse(json_string, protocol.timesheets_response_decoder())
    |> should.be_ok

  list.length(result) |> should.equal(2)

  let assert Ok(first) = list.first(result)
  first.duration |> should.equal(duration.Minutes(0))

  let assert Ok(second) = list.last(result)
  second.duration |> should.equal(duration.Minutes(240))
}

pub fn should_have_three_slots_test() {
  let result =
    json.parse(timesheet_with_slots_json_string, protocol.timesheet_decoder())
    |> should.be_ok

  result.slots
  |> list.length
  |> should.equal(3)
}

pub fn should_have_expected_slots_test() {
  let result =
    json.parse(timesheet_with_slots_json_string, protocol.timesheet_decoder())
    |> should.be_ok

  let expected = [
    timesheet.TimeSlot(
      project.ProjectId("p-01"),
      project.ActivityId("a-01"),
      24,
      1,
    ),
    timesheet.TimeSlot(
      project.ProjectId("p-01"),
      project.ActivityId("a-02"),
      29,
      2,
    ),
    timesheet.TimeSlot(
      project.ProjectId("p-02"),
      project.ActivityId("a-03"),
      35,
      3,
    ),
  ]

  result.slots
  |> should.equal(expected)
}

const timesheet_with_slots_json_string = "{
    \"date\": \"2026-03-02\",
    \"totalMinutes\": 90,
    \"timeSlots\": [
      {
        \"projectId\": \"p-01\",
        \"activityId\": \"a-01\",
        \"offset\": 24,
        \"duration\": 1
      },
      {
        \"projectId\": \"p-01\",
        \"activityId\": \"a-02\",
        \"offset\": 29,
        \"duration\": 2
      },
      {
        \"projectId\": \"p-02\",
        \"activityId\": \"a-03\",
        \"offset\": 35,
        \"duration\": 3
      }
    ]
  }"
