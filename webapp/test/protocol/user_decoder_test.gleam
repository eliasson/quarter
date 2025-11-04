import gleam/json
import gleam/option
import gleam/result
import gleam/time/timestamp
import gleeunit/should
import protocol
import user
import util

pub fn decode_minimal_user_test() {
  let expected_created = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")

  let result =
    "{
      \"id\": \"001\",
      \"email\": \"alice@example.com\",
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
     }"
    |> json.parse(protocol.user_resource_decoder())

  let expected =
    Ok(user.User(
      "001",
      "alice@example.com",
      result.unwrap(expected_created, util.timestamp_zero()),
      option.None,
    ))

  should.equal(result, expected)
}

pub fn decode_only_created_user_test() {
  let expected_created = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")

  let result =
    "{
      \"id\": \"001\",
      \"email\": \"alice@example.com\",
      \"created\": \"2025-11-04T16:49:39.2993437Z\",
      \"updated\": null
     }"
    |> json.parse(protocol.user_resource_decoder())

  let expected =
    Ok(user.User(
      "001",
      "alice@example.com",
      result.unwrap(expected_created, util.timestamp_zero()),
      option.None,
    ))

  should.equal(result, expected)
}

pub fn decode_full_user_test() {
  let expected_created = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")
  let expected_updated = timestamp.parse_rfc3339("2025-11-04T20:00:00.0Z")

  let result =
    "{
      \"id\": \"001\",
      \"email\": \"alice@example.com\",
      \"created\": \"2025-11-04T16:49:39.2993437Z\",
      \"updated\": \"2025-11-04T20:00:00.0Z\"
     }"
    |> json.parse(protocol.user_resource_decoder())

  let expected =
    Ok(user.User(
      "001",
      "alice@example.com",
      result.unwrap(expected_created, util.timestamp_zero()),
      option.Some(result.unwrap(expected_updated, util.timestamp_zero())),
    ))

  should.equal(result, expected)
}
