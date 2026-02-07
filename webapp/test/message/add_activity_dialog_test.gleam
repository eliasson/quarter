import gleam/option
import gleeunit/should
import message
import model
import test_util.{first}
import types
import webapp

pub fn should_update_name_field_in_dialog_state_test() {
  let project = test_util.arbitrary_project()
  let dialog = model.new_activity_dialog(project)

  let value =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog))
    |> first
    |> webapp.update(
      message.FormTextFieldUpdated(types.FormValue("name", "New Activity")),
    )
    |> first
    |> test_util.get_dialog_value("name")

  should.equal(value, option.Some("New Activity"))
}

pub fn should_update_description_field_in_dialog_state_test() {
  let project = test_util.arbitrary_project()
  let dialog = model.new_activity_dialog(project)

  let value =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog))
    |> first
    |> webapp.update(
      message.FormTextFieldUpdated(types.FormValue(
        "description",
        "Activity description",
      )),
    )
    |> first
    |> test_util.get_dialog_value("description")

  should.equal(value, option.Some("Activity description"))
}

pub fn should_update_color_field_in_dialog_state_test() {
  let project = test_util.arbitrary_project()
  let dialog = model.new_activity_dialog(project)

  let value =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog))
    |> first
    |> webapp.update(
      message.FormTextFieldUpdated(types.FormValue("color", "#FF5733")),
    )
    |> first
    |> test_util.get_dialog_value("color")

  should.equal(value, option.Some("#FF5733"))
}
