import gleeunit/should
import message
import model
import webapp

pub fn when_opening_main_menu_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenMainMenu)
    |> first

  should.equal(m.dropdowns, [model.MainMenu])
}

fn first(t: #(a, b)) {
  t.0
}
