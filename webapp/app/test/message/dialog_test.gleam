import app
import dialogs/project_dialog
import gleeunit/should
import message
import model
import quarter_shared/domain/input_value
import test_util.{first}

const dialog_one = model.AddProjectDialog(
  project_dialog.State(
    input_value.ValidValue("One"),
    input_value.ValidValue(""),
    False,
  ),
)

const dialog_two = model.AddProjectDialog(
  project_dialog.State(
    input_value.ValidValue("Two"),
    input_value.ValidValue(""),
    False,
  ),
)

pub fn when_opening_dialog_test() {
  let m =
    model.initial_model()
    |> app.update(message.OpenDialog(dialog_one))
    |> first

  should.equal(m.dialogs, [dialog_one])
}

pub fn when_closing_modal_with_one_open_dialog_test() {
  let m =
    model.initial_model()
    |> app.update(message.OpenDialog(dialog_one))
    |> first
    |> app.update(message.CloseModal)
    |> first

  should.equal(m.dialogs, [])
}

pub fn when_closing_modal_with_multiple_open_dialogs_test() {
  let m =
    model.initial_model()
    |> app.update(message.OpenDialog(dialog_one))
    |> first
    |> app.update(message.OpenDialog(dialog_two))
    |> first
    |> app.update(message.CloseModal)
    |> first

  should.equal(m.dialogs, [dialog_one])
}

pub fn when_opening_modal_with_open_dropdown_test() {
  let m =
    model.initial_model()
    |> app.update(message.OpenDropDownMenu("one"))
    |> first
    |> app.update(message.OpenDialog(dialog_one))
    |> first

  should.equal(m.dropdowns, [])
}

pub fn when_confirming_a_dialog_test() {
  let m =
    model.initial_model()
    |> app.update(message.OpenDialog(dialog_one))
    |> first
    |> app.update(message.ConfirmDialog)
    |> first

  should.equal(m.dialogs, [])
}
