import domain/color
import domain/project
import gleam/json
import gleam/option
import gleam/result
import gleam/time/timestamp
import gleeunit/should
import protocol
import util

pub fn decode_minimal_activity_test() {
  let expected_created = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")

  let result =
    "{
      \"id\": \"A01\",
      \"projectId\": \"P01\",
      \"name\": \"Activity Alpha\",
      \"description\": \"\",
      \"color\": \"#8E87F5\",
      \"isArchived\": false,
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
     }"
    |> json.parse(protocol.activity_decoder())

  let expected =
    Ok(project.Activity(
      project.ActivityId("A01"),
      project.ProjectId("P01"),
      "Activity Alpha",
      "",
      color.Color(142, 135, 245),
      False,
      result.unwrap(expected_created, util.timestamp_zero()),
      option.None,
    ))

  should.equal(result, expected)
}

pub fn decode_full_activity_test() {
  let expected_created = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")
  let expected_updated = timestamp.parse_rfc3339("2025-11-04T20:00:00.0Z")

  let result =
    "{
      \"id\": \"A01\",
      \"projectId\": \"P01\",
      \"name\": \"Activity Alpha\",
      \"description\": \"The alpha activity\",
      \"color\": \"#8E87F5\",
      \"isArchived\": true,
      \"created\": \"2025-11-04T16:49:39.2993437Z\",
      \"updated\": \"2025-11-04T20:00:00.0Z\"
     }"
    |> json.parse(protocol.activity_decoder())

  let expected =
    Ok(project.Activity(
      project.ActivityId("A01"),
      project.ProjectId("P01"),
      "Activity Alpha",
      "The alpha activity",
      color.Color(142, 135, 245),
      True,
      result.unwrap(expected_created, util.timestamp_zero()),
      option.Some(result.unwrap(expected_updated, util.timestamp_zero())),
    ))

  should.equal(result, expected)
}

pub fn decode_invalid_activity_test() {
  let result =
    "{
      \"id\": \"A01\",
      \"projectId\": \"P01\",
      \"name\": \"Activity Alpha\",
      \"description\": \"\",
      \"color\": \"redish\",
      \"isArchived\": false,
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
     }"
    |> json.parse(protocol.activity_decoder())

  should.be_error(result)
}
