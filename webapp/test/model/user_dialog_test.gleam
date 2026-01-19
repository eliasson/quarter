import domain/email
import gleeunit/should
import model.{InvalidValue, UnvalidatedValue}

pub fn should_be_valid_state_test() {
  let state =
    model.UserDialogState(
      UnvalidatedValue(email.Email("jane@example.com")),
      True,
    )
  let updated = model.validate_user_dialog_state(state)

  should.be_true(updated.is_valid)
}

pub fn should_be_invalid_state_with_one_invalid_value_test() {
  let state =
    model.UserDialogState(UnvalidatedValue(email.Email("jane@")), True)
  let updated = model.validate_user_dialog_state(state)

  should.be_false(updated.is_valid)
}

pub fn should_include_validation_error_in_email_value_test() {
  let state =
    model.UserDialogState(UnvalidatedValue(email.Email("jane@")), True)
  let updated = model.validate_user_dialog_state(state)

  case updated.email {
    InvalidValue(_, errors) -> should.equal(errors, ["Invalid e-mail address"])
    _ -> should.fail()
  }
}
