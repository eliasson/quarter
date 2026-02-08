import dialogs/user_dialog
import domain/email
import domain/input_value.{InvalidValue, UnvalidatedValue}
import gleam/list
import gleeunit/should

pub fn should_be_valid_state_test() {
  let state =
    user_dialog.State(UnvalidatedValue(email.Email("jane@example.com")), True)
  let updated = user_dialog.validate(state)

  should.be_true(updated.is_valid)
}

pub fn should_be_invalid_test() {
  let states = [
    user_dialog.State(UnvalidatedValue(email.Email("jane@")), True),
  ]

  list.each(states, fn(state) {
    user_dialog.validate(state).is_valid
    |> should.be_false()
  })
}

pub fn should_include_validation_error_in_email_value_test() {
  let state = user_dialog.State(UnvalidatedValue(email.Email("jane@")), True)
  let updated = user_dialog.validate(state)

  case updated.email {
    InvalidValue(_, errors) -> should.equal(errors, ["Invalid e-mail address"])
    _ -> should.fail()
  }
}
