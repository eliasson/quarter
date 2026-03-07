import gleam/float
import gleam/time/duration
import gleam/time/timestamp
import gleeunit/should
import model
import route

pub fn should_have_unauthenticated_user_test() {
  let m = model.initial_model()

  should.be_false(m.is_authenticated)
}

pub fn should_have_home_as_initial_route_test() {
  let m = model.initial_model()

  should.equal(m.route, route.Home)
}

pub fn should_have_no_errors_test() {
  let m = model.initial_model()
  should.equal(m.errors, [])
}

pub fn should_set_today_from_system_time_test() {
  let now = timestamp.system_time()
  let m = model.initial_model()

  let seconds =
    m.today
    |> timestamp.difference(now)
    |> duration.to_seconds
    |> float.truncate

  should.be_true(seconds < 1)
}

pub fn should_not_have_any_selected_activity() {
  model.initial_model().selected_activity
  |> should.be_none
}

pub fn should_have_start_of_day_set_to_six_test() {
  model.initial_model().start_of_day
  |> should.equal(6)
}

pub fn should_have_end_of_day_set_to_18_test() {
  model.initial_model().end_of_day
  |> should.equal(18)
}
