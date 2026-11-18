import admin
import gleam/option
import gleeunit/should
import message
import model
import quarter_shared/types
import test_util.{first}

pub fn should_update_email_in_dialog_state_test() {
  let initial_dialog = model.new_user_dialog()

  let value =
    model.initial_model()
    |> admin.update(message.OpenDialog(initial_dialog))
    |> first
    |> admin.update(
      message.FormTextFieldUpdated(types.FormValue("email", "jane@example.com")),
    )
    |> first
    |> test_util.get_dialog_value("email")

  should.equal(value, option.Some("jane@example.com"))
}
