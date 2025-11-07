import gleeunit/should
import message
import model
import route
import test_util.{first}
import webapp

pub fn when_navigating_with_open_main_menu_test() {
  let m =
    model.initial_model()
    |> webapp.update(message.OpenDropDownMenu("arbitrary"))
    |> first
    |> webapp.update(message.OnRouteChange(route.Report))
    |> first

  should.equal(m.dropdowns, [])
}
