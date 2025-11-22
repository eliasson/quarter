import gleeunit/should
import model
import util

pub fn should_be_valid_state_test() {
  let state = model.UserDialogState(util.Email("jane@example.com"), [], True)
  let updated = model.validate_user_dialog_state(state)

  should.be_true(updated.is_valid)
}
