import gleam/option
import gleeunit/should
import message
import model
import quarter_shared/types
import test_util.{first}
import app

pub fn should_update_field_in_dialog_state_test() {
  let project = test_util.arbitrary_project()
  let dialog = model.edit_project_dialog(project)

  let value =
    model.initial_model()
    |> app.update(message.OpenDialog(dialog))
    |> first
    |> app.update(
      message.FormTextFieldUpdated(types.FormValue("name", "Test project")),
    )
    |> first
    |> test_util.get_dialog_value("name")

  should.equal(value, option.Some("Test project"))
}
