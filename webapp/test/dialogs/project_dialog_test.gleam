import dialogs/project_dialog
import domain/input_value.{ValidValue}
import gleeunit/should

pub fn should_be_valid_state_test() {
  let state =
    project_dialog.State(
      ValidValue("My Project"),
      ValidValue("A description"),
      False,
    )
  let updated = project_dialog.validate(state)

  should.be_true(updated.is_valid)
}

pub fn should_be_invalid_if_name_is_empty_test() {
  let state =
    project_dialog.State(ValidValue(""), ValidValue("A description"), True)
  let updated = project_dialog.validate(state)

  should.be_false(updated.is_valid)
}

pub fn should_be_invalid_if_description_is_empty_test() {
  let state =
    project_dialog.State(ValidValue("My Project"), ValidValue(""), True)
  let updated = project_dialog.validate(state)

  should.be_false(updated.is_valid)
}

pub fn should_be_invalid_if_both_are_empty_test() {
  let state = project_dialog.State(ValidValue(""), ValidValue(""), True)
  let updated = project_dialog.validate(state)

  should.be_false(updated.is_valid)
}
