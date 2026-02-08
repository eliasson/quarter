import dialogs/project_dialog
import domain/input_value.{ValidValue}
import gleam/list
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

pub fn should_be_invalid_test() {
  let states = [
    project_dialog.State(ValidValue(""), ValidValue("A description"), True),
    project_dialog.State(ValidValue("My Project"), ValidValue(""), True),
    project_dialog.State(ValidValue(""), ValidValue(""), True),
  ]

  list.each(states, fn(state) {
    project_dialog.validate(state).is_valid
    |> should.be_false()
  })
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
