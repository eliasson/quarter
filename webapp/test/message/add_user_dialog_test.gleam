import gleam/option
import gleeunit/should
import message
import model
import test_util.{first}
import types
import webapp

pub fn should_update_email_in_dialog_state_test() {
  let initial_dialog = model.new_user_dialog()

  let value =
    model.initial_model()
    |> webapp.update(message.OpenDialog(initial_dialog))
    |> first
    |> webapp.update(
      message.FormTextFieldUpdated(types.FormValue("email", "jane@example.com")),
    )
    |> first
    |> model.get_dialog_value("email")

  should.equal(value, option.Some("jane@example.com"))
}
