import gleeunit/should
import message
import model
import test_util.{first}
import webapp

pub fn when_opening_main_menu_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenMainMenu)
    |> first

  should.equal(m.dropdowns, [model.MainMenu])
}

pub fn when_closing_main_menu_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenMainMenu)
    |> first
    |> webapp.update(message.CloseModal)
    |> first

  should.equal(m.dropdowns, [])
}
