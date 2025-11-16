import gleeunit/should
import message
import model
import test_util.{first}
import util.{Email}
import webapp

const dialog_one = model.AddUserDialog(
  model.UserDialogState(Email("one@example.com")),
)

const dialog_two = model.AddUserDialog(
  model.UserDialogState(Email("two@example.com")),
)

pub fn when_opening_dialog_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog_one))
    |> first

  should.equal(m.dialogs, [dialog_one])
}

pub fn when_closing_modal_with_one_open_dialog_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog_one))
    |> first
    |> webapp.update(message.CloseModal)
    |> first

  should.equal(m.dialogs, [])
}

pub fn when_closing_modal_with_multiple_open_dialogs_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog_one))
    |> first
    |> webapp.update(message.OpenDialog(dialog_two))
    |> first
    |> webapp.update(message.CloseModal)
    |> first

  should.equal(m.dialogs, [dialog_one])
}

pub fn when_opening_modal_with_open_dropdown_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDropDownMenu("one"))
    |> first
    |> webapp.update(message.OpenDialog(dialog_one))
    |> first

  should.equal(m.dropdowns, [])
}

pub fn when_confirming_a_dialog_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDialog(dialog_one))
    |> first
    |> webapp.update(message.ConfirmDialog)
    |> first

  should.equal(m.dialogs, [])
}
