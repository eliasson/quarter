import gleam/option
import gleeunit/should
import message
import model
import test_util.{first}
import types
import webapp

pub fn should_update_field_in_dialog_state_test() {
  let dialog = model.new_project_dialog()

  let value =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog))
    |> first
    |> webapp.update(
      message.FormTextFieldUpdated(types.FormValue("name", "New Project")),
    )
    |> first
    |> test_util.get_dialog_value("name")

  should.equal(value, option.Some("New Project"))
}
