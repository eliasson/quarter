import gleeunit/should
import model.{initial_model, navigate_to}
import route

pub fn should_have_unauthenticated_user_test() {
  let model = initial_model()

  should.be_false(model.is_authenticated)
}

pub fn should_have_home_as_initial_route_test() {
  let model = initial_model()

  should.equal(model.route, route.Home)
}

pub fn should_navigate_to_route_test() {
  let model = initial_model() |> navigate_to(route.Timesheet)

  should.equal(model.route, route.Timesheet)
}
