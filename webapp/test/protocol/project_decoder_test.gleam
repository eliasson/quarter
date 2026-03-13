import domain/project
import gleam/json
import gleam/option
import gleam/result
import gleam/time/timestamp
import gleeunit/should
import protocol
import util/timestamp as tsutil

pub fn decode_minimal_project_test() {
  let expected_created = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")

  let result =
    "{
      \"id\": \"001\",
      \"name\": \"Project Alpha\",
      \"description\": \"\",
      \"isArchived\": false,
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
     }"
    |> json.parse(protocol.project_decoder())

  let expected =
    Ok(
      project.Project(
        project.ProjectId("001"),
        "Project Alpha",
        "",
        False,
        result.unwrap(expected_created, tsutil.timestamp_zero()),
        option.None,
        [],
      ),
    )

  should.equal(result, expected)
}

pub fn decode_full_project_test() {
  let expected_created = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")
  let expected_updated = timestamp.parse_rfc3339("2025-11-04T20:00:00.0Z")

  let result =
    "{
      \"id\": \"001\",
      \"name\": \"Project Alpha\",
      \"description\": \"The alpha project\",
      \"isArchived\": true,
      \"created\": \"2025-11-04T16:49:39.2993437Z\",
      \"updated\": \"2025-11-04T20:00:00.0Z\"
     }"
    |> json.parse(protocol.project_decoder())

  let expected =
    Ok(
      project.Project(
        project.ProjectId("001"),
        "Project Alpha",
        "The alpha project",
        True,
        result.unwrap(expected_created, tsutil.timestamp_zero()),
        option.Some(result.unwrap(expected_updated, tsutil.timestamp_zero())),
        [],
      ),
    )

  should.equal(result, expected)
}
