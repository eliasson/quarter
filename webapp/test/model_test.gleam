import gleam/float
import gleam/time/duration
import gleam/time/timestamp
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

pub fn should_have_no_errors_test() {
  let model = initial_model()
  should.equal(model.errors, [])
}

pub fn should_set_today_from_system_time_test() {
  let now = timestamp.system_time()
  let model = initial_model()

  let seconds =
    model.today
    |> timestamp.difference(now)
    |> duration.to_seconds
    |> float.truncate

  should.be_true(seconds < 1)
}
