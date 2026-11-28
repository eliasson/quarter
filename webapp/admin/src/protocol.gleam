import domain
import gleam/dynamic/decode
import gleam/json
import gleam/option
import lustre/effect.{type Effect}
import quarter_shared/domain/email
import quarter_shared/domain/user
import quarter_shared/protocol as shared_protocol
import rsvp

/// Get all the system users.
/// This HTTP call requires the current user to have sufficient access to do
/// so, else the request will fail with Forbidden.
pub fn get_system_users(
  on_response handle_response: fn(Result(List(domain.UserListItem), rsvp.Error)) ->
    msg,
) -> Effect(msg) {
  let url = "/api/users"

  let handler =
    rsvp.expect_json(decode.list(user_list_item_decoder()), handle_response)

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

/// Decoder for `UserListItemResourceOutput` returned by `GET /api/users`.
pub fn user_list_item_decoder() -> decode.Decoder(domain.UserListItem) {
  use id <- decode.field("id", decode.string)
  use email <- decode.field("email", decode.string)
  use created <- decode.field("created", shared_protocol.decode_timestamp())
  use updated <- decode.optional_field(
    "updated",
    option.None,
    shared_protocol.decode_optional_timestamp(),
  )
  use roles <- decode.optional_field("roles", [], decode.list(decode.string))
  use last_login <- decode.optional_field(
    "lastLogin",
    option.None,
    shared_protocol.decode_optional_timestamp(),
  )

  decode.success(domain.UserListItem(
    id:,
    email:,
    created:,
    updated:,
    roles:,
    last_login:,
  ))
}
