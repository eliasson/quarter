import dialogs/project_dialog
import domain/input_value.{ValidValue}
import gleeunit/should
import test_util

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

pub fn should_create_state_for_new_test() {
  let state = project_dialog.new()

  should.equal(state.name.value, "")
  should.equal(state.description.value, "")
}

pub fn should_create_state_for_edit_test() {
  let project = test_util.arbitrary_project()
  let state = project_dialog.edit(project)

  should.equal(state.name.value, project.name)
  should.equal(state.description.value, project.description)
}
