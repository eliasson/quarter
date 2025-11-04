import gleam/dynamic/decode
import lustre/effect.{type Effect}
import message
import rsvp
import user

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

pub fn user_resource_decoder() {
  use id <- decode.field("id", decode.string)
  use email <- decode.field("email", decode.string)

  decode.success(user.User(id:, email:))
}
