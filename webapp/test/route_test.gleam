import gleam/list
import gleam/option
import gleam/uri
import gleeunit/should
import route

pub fn should_identify_uri_test() {
  let tests = [
    #("", route.Home),
    #("/timesheet", route.Home),
    #("/ui/timesheet", route.Timesheet),
    #("/ui/manage", route.Manage),
    #("/ui/report", route.Report),
    #("/ui/admin/users", route.AdministerSystemUsers),
    #("/ui/admin/features", route.AdministerSystemFeatures),
  ]

  list.each(tests, fn(t) { should.equal(route.identify(new_uri(t.0)), t.1) })
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
