import dialogs/user_dialog
import gleeunit/should
import message
import model
import quarter_shared/domain/email
import quarter_shared/domain/input_value
import test_util.{first}
import app

const dialog_one = model.AddUserDialog(
  user_dialog.State(
    input_value.ValidValue(email.Email("one@example.com")),
    False,
  ),
)

const dialog_two = model.AddUserDialog(
  user_dialog.State(
    input_value.ValidValue(email.Email("two@example.com")),
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
