import domain
import gleam/dynamic/decode
import gleam/json
import gleam/option
import gleam/time/timestamp
import gleeunit/should
import protocol

pub fn decode_minimal_user_list_item_test() {
  let assert Ok(created) =
    timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")

  let result =
    "{
      \"id\": \"001\",
      \"email\": \"alice@example.com\",
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
     }"
    |> json.parse(protocol.user_list_item_decoder())

  let expected =
    Ok(domain.UserListItem(
      id: "001",
      email: "alice@example.com",
      created:,
      updated: option.None,
      roles: [],
      last_login: option.None,
    ))

  should.equal(result, expected)
}

pub fn decode_user_list_item_with_null_values_test() {
  let assert Ok(created) =
    timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")

  let result =
    "{
      \"id\": \"001\",
      \"email\": \"alice@example.com\",
      \"created\": \"2025-11-04T16:49:39.2993437Z\",
      \"updated\": null,
      \"roles\": [],
      \"lastLogin\": null
     }"
    |> json.parse(protocol.user_list_item_decoder())

  let expected =
    Ok(domain.UserListItem(
      id: "001",
      email: "alice@example.com",
      created:,
      updated: option.None,
      roles: [],
      last_login: option.None,
    ))

  should.equal(result, expected)
}

pub fn decode_full_user_list_item_test() {
  let assert Ok(created) =
    timestamp.parse_rfc3339("2025-11-04T16:49:39.2993437Z")
  let assert Ok(updated) = timestamp.parse_rfc3339("2025-11-04T20:00:00.0Z")
  let assert Ok(last_login) = timestamp.parse_rfc3339("2025-11-27T08:15:00.0Z")

  let result =
    "{
      \"id\": \"001\",
      \"email\": \"alice@example.com\",
      \"created\": \"2025-11-04T16:49:39.2993437Z\",
      \"updated\": \"2025-11-04T20:00:00.0Z\",
      \"roles\": [\"administrator\"],
      \"lastLogin\": \"2025-11-27T08:15:00.0Z\"
     }"
    |> json.parse(protocol.user_list_item_decoder())

  let expected =
    Ok(domain.UserListItem(
      id: "001",
      email: "alice@example.com",
      created:,
      updated: option.Some(updated),
      roles: ["administrator"],
      last_login: option.Some(last_login),
    ))

  should.equal(result, expected)
}

pub fn decode_list_of_user_list_items_test() {
  let result =
    "[
      {
        \"id\": \"001\",
        \"email\": \"alice@example.com\",
        \"created\": \"2025-11-04T16:49:39.2993437Z\",
        \"updated\": null,
        \"roles\": [\"administrator\"],
        \"lastLogin\": \"2025-11-27T08:15:00.0Z\"
      },
      {
        \"id\": \"002\",
        \"email\": \"bob@example.com\",
        \"created\": \"2025-11-05T10:00:00.0Z\",
        \"updated\": null,
        \"roles\": [],
        \"lastLogin\": null
      }
     ]"
    |> json.parse(decode.list(protocol.user_list_item_decoder()))

  let assert Ok([alice, bob]) = result
  should.equal(alice.email, "alice@example.com")
  should.be_true(option.is_some(alice.last_login))
  should.equal(bob.email, "bob@example.com")
  should.equal(bob.last_login, option.None)
}

pub fn fail_on_invalid_last_login_test() {
  "{
    \"id\": \"001\",
    \"email\": \"alice@example.com\",
    \"created\": \"2025-11-04T16:49:39.2993437Z\",
    \"lastLogin\": \"not-a-timestamp\"
   }"
  |> json.parse(protocol.user_list_item_decoder())
  |> should.be_error()
}

pub fn fail_on_missing_created_test() {
  "{
    \"id\": \"001\",
    \"email\": \"alice@example.com\",
    \"lastLogin\": \"2025-11-27T08:15:00.0Z\"
   }"
  |> json.parse(protocol.user_list_item_decoder())
  |> should.be_error()
}
