import gleam/time/timestamp
import gleeunit/should
import model
import route
import util/timestamp as tsutil

pub fn navigate_to_home_route_test() {
  let now = timestamp.system_time()

  let m =
    model.initial_model()
    |> model.go_to_next_month
    |> model.navigate_to(route.Home)

  m.today
  |> tsutil.to_iso_date
  |> should.equal(tsutil.to_iso_date(now))
}

pub fn navigate_to_timesheet_route_test() {
  let now = timestamp.system_time()

  let m =
    model.initial_model()
    |> model.go_to_next_month
    |> model.navigate_to(route.Timesheet(now))

  m.today
  |> tsutil.to_iso_date
  |> should.equal(tsutil.to_iso_date(now))
}

pub fn should_navigate_to_route_test() {
  let m = model.initial_model() |> model.navigate_to(route.Manage)

  should.equal(m.route, route.Manage)
}
