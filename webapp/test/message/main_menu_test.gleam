import gleeunit/should
import message
import model
import test_util.{first}
import webapp

pub fn when_opening_main_menu_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDropDownMenu("main"))
    |> first

  should.equal(m.dropdowns, [model.DropDownMenu("main")])
}

pub fn when_closing_main_menu_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDropDownMenu("main"))
    |> first
    |> webapp.update(message.CloseModal)
    |> first

  should.equal(m.dropdowns, [])
}
