import domain/email
import gleeunit/should
import message
import model.{ValidValue}
import test_util.{first}
import webapp

const dialog_one = model.AddUserDialog(
  model.UserDialogState(ValidValue(email.Email("one@example.com")), False),
)

const dialog_two = model.AddUserDialog(
  model.UserDialogState(ValidValue(email.Email("two@example.com")), False),
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
