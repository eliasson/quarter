import gleam/dynamic/decode
import gleam/json
import lustre/effect.{type Effect}
import quarter_shared/domain/email
import quarter_shared/domain/user
import quarter_shared/protocol as shared_protocol
import rsvp

/// Get all the system users.
/// This HTTP call requires the current user to have sufficient access to do
/// so, else the request will fail with Forbidden.
pub fn get_system_users(
  on_response handle_response: fn(Result(List(user.User), rsvp.Error)) -> msg,
) -> Effect(msg) {
  let url = "/api/users"

  let handler =
    rsvp.expect_json(
      decode.list(shared_protocol.user_resource_decoder()),
      handle_response,
    )

  rsvp.get(url, handler)
}

pub fn add_user(
  email: email.Email,
  on_response handle_response: fn(Result(user.User, rsvp.Error)) -> msg,
) -> Effect(msg) {
  let url = "/api/users"
  let handler =
    rsvp.expect_json(shared_protocol.user_resource_decoder(), handle_response)
  let payload = json.object([#("email", json.string(email.value))])

  rsvp.post(url, payload, handler)
}
