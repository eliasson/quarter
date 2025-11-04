import gleam/json
import gleeunit/should
import protocol
import user

pub fn decode_minimal_user_test() {
  let result =
    "{
      \"id\": \"001\",
      \"email\": \"alice@example.com\"
    }"
    |> json.parse(protocol.user_resource_decoder())

  let expected = Ok(user.User("001", "alice@example.com"))
  should.equal(result, expected)
}
