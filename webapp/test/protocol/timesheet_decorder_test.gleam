import domain/duration
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
