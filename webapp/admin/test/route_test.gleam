import gleam/list
import gleam/uri
import gleeunit/should
import route

pub fn should_identify_route_test() {
  let tests = [
    #("/admin", route.Users),
    #("/admin/users", route.Users),
    #("/admin/unknown", route.Users),
  ]

  list.each(tests, fn(t) {
    let assert Ok(u) = uri.parse(t.0)
    should.equal(route.identify(u), t.1)
  })
}

pub fn should_get_url_for_route_test() {
  route.to_url(route.Users)
  |> should.equal("/admin/users")
}
