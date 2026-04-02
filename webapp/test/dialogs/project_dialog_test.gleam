import dialogs/project_dialog
import domain/color
import domain/input_value.{ValidValue}
import gleam/list
import gleeunit/should
import test_util

const valid_color = ValidValue(color.Color(69, 123, 157))

pub fn should_be_valid_state_test() {
  let states = [
    project_dialog.State(
      ValidValue("My Project"),
      ValidValue("A description"),
      valid_color,
      False,
    ),
    project_dialog.State(
      ValidValue("My Project"),
      ValidValue(""),
      valid_color,
      False,
    ),
  ]

  list.each(states, fn(state) {
    project_dialog.validate(state).is_valid
    |> should.be_true()
  })
}

pub fn should_be_invalid_test() {
  let states = [
    project_dialog.State(
      ValidValue(""),
      ValidValue("A description"),
      valid_color,
      True,
    ),
    project_dialog.State(ValidValue(""), ValidValue(""), valid_color, True),
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
