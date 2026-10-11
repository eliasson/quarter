//// HTTP calls and decoders shared by the Quarter web applications.

import gleam/dynamic/decode
import gleam/option.{type Option, None, Some}
import gleam/time/timestamp
import lustre/effect.{type Effect}
import quarter_shared/domain/user
import quarter_shared/util/timestamp as tsutil
import rsvp

/// Get the currently logged in user, if logged in.
pub fn get_current_user(
  on_response handle_response: fn(Result(user.User, rsvp.Error)) -> msg,
) -> Effect(msg) {
  let url = "/api/users/self"
  let handler = rsvp.expect_json(user_resource_decoder(), handle_response)

  rsvp.get(url, handler)
}

// Decoders ------------------------------------------------------------------

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
pub fn decode_timestamp() -> decode.Decoder(timestamp.Timestamp) {
  use ts_str <- decode.then(decode.string)

  case timestamp.parse_rfc3339(ts_str) {
    Ok(ts) -> decode.success(ts)
    _ -> decode.failure(tsutil.timestamp_zero(), "Could not parse timestamp")
  }
}

/// Decode a ISO-8601 / RFC-3339 date (without time) as a timestamp at midnight UTC.
pub fn decode_timestamp_from_date() -> decode.Decoder(timestamp.Timestamp) {
  use ts_str <- decode.then(decode.string)

  case timestamp.parse_rfc3339(ts_str <> "T00:00:00Z") {
    Ok(ts) -> decode.success(ts)
    _ -> decode.failure(tsutil.timestamp_zero(), "Could not parse timestamp")
  }
}

/// Decode an optional ISO-8601 / RFC-3339 timestamp from a string.
pub fn decode_optional_timestamp() -> decode.Decoder(
  Option(timestamp.Timestamp),
) {
  use ts_str <- decode.then(decode.optional(decode.string))

  case ts_str {
    Some(ts) ->
      case timestamp.parse_rfc3339(ts) {
        Ok(t) -> decode.success(Some(t))
        _ -> decode.failure(None, "Could not parse timestamp")
      }

    None -> decode.success(None)
  }
}
