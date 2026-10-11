import gleam/option
import gleeunit/should
import message
import model
import quarter_shared/types
import test_util.{first}
import app

pub fn should_update_field_in_dialog_state_test() {
  let dialog = model.new_project_dialog()

  let value =
    model.initial_model()
    |> app.update(message.OpenDialog(dialog))
    |> first
    |> app.update(
      message.FormTextFieldUpdated(types.FormValue("name", "New Project")),
    )
    |> first
    |> test_util.get_dialog_value("name")

  should.equal(value, option.Some("New Project"))
}
