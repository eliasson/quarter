import gleam/dynamic/decode
import gleam/io
import gleam/option
import gleam/time/timestamp
import lustre/effect.{type Effect}
import message
import rsvp
import user
import util

/// Get the currently logged in user, if logged in.
pub fn get_current_user(
  on_response handle_response: fn(Result(user.User, rsvp.Error)) -> message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/users/self"
  let handler = rsvp.expect_json(user_resource_decoder(), handle_response)

  rsvp.get(url, handler)
}

/// Get all the system users.
/// This HTTP call requires the current user to have sufficient access to do
/// so, else the request will fail with Forbidden.
pub fn get_system_users(
  on_response handle_response: fn(Result(List(user.User), rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/users"

  let handler =
    rsvp.expect_json(decode.list(user_resource_decoder()), handle_response)

  rsvp.get(url, handler)
}

pub fn user_resource_decoder() -> decode.Decoder(user.User) {
  use id <- decode.field("id", decode.string)
  use email <- decode.field("email", decode.string)
  use created <- decode.field("created", decode_timestamp())
  use updated <- decode.optional_field(
    "updated",
    option.None,
    decode_optional_timestamp(),
  )

  decode.success(user.User(id:, email:, created:, updated:))
}

/// Decode a ISO-8601 / RFC-3339 timestamp from a string.
fn decode_timestamp() -> decode.Decoder(timestamp.Timestamp) {
  use ts_str <- decode.then(decode.string)

  case timestamp.parse_rfc3339(ts_str) {
    Ok(ts) -> decode.success(ts)
    _ ->
      decode.failure(
        util.timestamp_zero(),
        "Could not parse the \"created\" timestamp",
      )
  }
}

fn decode_optional_timestamp() -> decode.Decoder(
  option.Option(timestamp.Timestamp),
) {
  use ts_str <- decode.then(decode.optional(decode.string))

  case ts_str {
    option.Some(ts) -> {
      case timestamp.parse_rfc3339(ts) {
        Ok(t) -> decode.success(option.Some(t))
        _ -> decode.success(option.None)
      }
    }
    option.None -> decode.success(option.None)
  }
}
