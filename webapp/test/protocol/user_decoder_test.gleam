import gleam/json
import gleam/result
import gleam/time/timestamp
import gleeunit/should
import protocol
import user

pub fn decode_minimal_user_test() {
  let expected_ts = timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")

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
      result.unwrap(expected_ts, timestamp.from_unix_seconds(0)),
    ))

  should.equal(result, expected)
}
