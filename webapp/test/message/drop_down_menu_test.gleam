import gleeunit/should
import message
import model
import test_util.{first}
import webapp

pub fn when_opening_drop_down_menu_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDropDownMenu("main"))
    |> first

  should.equal(m.dropdowns, [model.DropDownMenu("main")])
}

pub fn when_closing_modal_with_one_drop_down_menu_open_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDropDownMenu("main"))
    |> first
    |> webapp.update(message.CloseModal)
    |> first

  should.equal(m.dropdowns, [])
}

pub fn when_closing_modal_with_multiple_drop_down_menus_open_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDropDownMenu("one"))
    |> first
    |> webapp.update(message.OpenDropDownMenu("two"))
    |> first
    |> webapp.update(message.OpenDropDownMenu("three"))
    |> first
    |> webapp.update(message.CloseModal)
    |> first

  should.equal(m.dropdowns, [
    model.DropDownMenu("one"),
    model.DropDownMenu("two"),
  ])
}
