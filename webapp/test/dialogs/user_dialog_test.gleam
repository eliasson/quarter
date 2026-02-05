import dialogs/user_dialog
import domain/email
import domain/input_value.{InvalidValue, UnvalidatedValue}
import gleeunit/should

pub fn should_be_valid_state_test() {
  let state =
    user_dialog.State(UnvalidatedValue(email.Email("jane@example.com")), True)
  let updated = user_dialog.validate(state)

  should.be_true(updated.is_valid)
}

pub fn should_be_invalid_state_with_one_invalid_value_test() {
  let state = user_dialog.State(UnvalidatedValue(email.Email("jane@")), True)
  let updated = user_dialog.validate(state)

  should.be_false(updated.is_valid)
}

pub fn should_include_validation_error_in_email_value_test() {
  let state = user_dialog.State(UnvalidatedValue(email.Email("jane@")), True)
  let updated = user_dialog.validate(state)

  case updated.email {
    InvalidValue(_, errors) -> should.equal(errors, ["Invalid e-mail address"])
    _ -> should.fail()
  }
}
