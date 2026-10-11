import gleeunit/should
import message
import model
import route
import test_util.{first}
import app

pub fn when_navigating_with_open_menu_test() {
  let m =
    model.initial_model()
    |> app.update(message.OpenDropDownMenu("arbitrary"))
    |> first
    |> app.update(message.OnRouteChange(route.Home))
    |> first

  should.equal(m.dropdowns, [])
}
