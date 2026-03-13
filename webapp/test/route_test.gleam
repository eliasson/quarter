import gleam/list
import gleam/option
import gleam/time/calendar
import gleam/time/timestamp
import gleam/uri
import gleeunit/should
import route
import util/timestamp as tsutil

pub fn should_identify_uri_test() {
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 1),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  let now = tsutil.with_zero_time(timestamp.system_time())

  let tests = [
    #("", route.Home),
    #("/timesheet", route.Home),
    #("/ui/timesheet", route.Timesheet(now)),
    #("/ui/timesheet/2026-02-01", route.Timesheet(ts)),
    #("/ui/manage", route.Manage),
    #("/ui/report", route.Report),
    #("/ui/admin/users", route.AdministerSystemUsers),
    #("/ui/admin/features", route.AdministerSystemFeatures),
  ]

  list.each(tests, fn(t) { should.equal(route.identify(new_uri(t.0)), t.1) })
}

pub fn should_generate_timesheet_uri_test() {
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 1),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  route.for_timesheet(ts)
  |> should.equal("/ui/timesheet/2026-02-01")
}

pub fn should_get_url_for_route_test() {
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 1),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  let tests = [
    #(route.Home, "/ui"),
    #(route.Timesheet(ts), "/ui/timesheet/2026-02-01"),
    #(route.Manage, "/ui/manage"),
    #(route.Report, "/ui/report"),
    #(route.AdministerSystemUsers, "/ui/admin/users"),
    #(route.AdministerSystemFeatures, "/ui/admin/features"),
  ]

  list.each(tests, fn(t) { should.equal(route.to_url(t.0), t.1) })
}

pub fn should_identify_active_route_test() {
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 1),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  let now = tsutil.with_zero_time(timestamp.system_time())

  let tests = [
    #(route.Home, route.Home, True),
    #(route.Home, route.Manage, False),
    #(route.Timesheet(now), route.Timesheet(now), True),
    #(route.Timesheet(now), route.Timesheet(ts), True),
    #(route.Timesheet(now), route.Manage, False),
  ]

  list.each(tests, fn(t) { should.equal(route.is_active(t.0, t.1), t.2) })
}

fn new_uri(path: String) {
  uri.Uri(
    option.Some("https"),
    option.None,
    option.Some("example.com"),
    option.None,
    path,
    option.None,
    option.None,
  )
}
