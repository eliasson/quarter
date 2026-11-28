import domain
import gleam/list
import gleam/option.{type Option, None, Some}
import gleam/order
import gleam/time/timestamp
import gleeunit/should

pub fn most_recently_seen_first_test() {
  let older =
    user_item(
      "a",
      created: "2026-11-01T00:00:00Z",
      last_login: Some("2026-12-01T00:00:00Z"),
    )
  let newer =
    user_item(
      "b",
      created: "2026-11-01T00:00:00Z",
      last_login: Some("2026-12-27T00:00:00Z"),
    )

  domain.compare_by_activity(newer, older) |> should.equal(order.Lt)
  domain.compare_by_activity(older, newer) |> should.equal(order.Gt)
}

pub fn never_logged_in_last_test() {
  let seen =
    user_item(
      "a",
      created: "2026-01-01T00:00:00Z",
      last_login: Some("2026-11-01T00:00:00Z"),
    )
  let never = user_item("b", created: "2026-06-01T00:00:00Z", last_login: None)

  domain.compare_by_activity(seen, never) |> should.equal(order.Lt)
  domain.compare_by_activity(never, seen) |> should.equal(order.Gt)
}

pub fn same_last_login_newest_join_first_test() {
  let early =
    user_item(
      "a",
      created: "2026-01-01T00:00:00Z",
      last_login: Some("2026-11-01T00:00:00Z"),
    )
  let late =
    user_item(
      "b",
      created: "2026-06-01T00:00:00Z",
      last_login: Some("2026-11-01T00:00:00Z"),
    )

  domain.compare_by_activity(late, early) |> should.equal(order.Lt)
}

pub fn never_logged_in_newest_join_first_test() {
  let early = user_item("a", created: "2026-01-01T00:00:00Z", last_login: None)
  let late = user_item("b", created: "2026-06-01T00:00:00Z", last_login: None)

  domain.compare_by_activity(late, early) |> should.equal(order.Lt)
}

pub fn equal_users_test() {
  let a = user_item("a", created: "2026-01-01T00:00:00Z", last_login: None)

  domain.compare_by_activity(a, a) |> should.equal(order.Eq)
}

pub fn sort_full_list_test() {
  let users = [
    user_item("never-old", created: "2026-01-01T00:00:00Z", last_login: None),
    user_item(
      "seen-old",
      created: "2026-02-01T00:00:00Z",
      last_login: Some("2026-10-01T00:00:00Z"),
    ),
    user_item("never-new", created: "2026-09-01T00:00:00Z", last_login: None),
    user_item(
      "seen-new",
      created: "2026-03-01T00:00:00Z",
      last_login: Some("2026-11-27T00:00:00Z"),
    ),
    user_item(
      "seen-old-tie",
      created: "2026-05-01T00:00:00Z",
      last_login: Some("2026-10-01T00:00:00Z"),
    ),
  ]

  users
  |> list.sort(domain.compare_by_activity)
  |> list.map(fn(u) { u.id })
  |> should.equal([
    "seen-new", "seen-old-tie", "seen-old", "never-new", "never-old",
  ])
}

fn ts(s: String) -> timestamp.Timestamp {
  let assert Ok(t) = timestamp.parse_rfc3339(s)
  t
}

fn user_item(
  id: String,
  created created: String,
  last_login last_login: Option(String),
) -> domain.UserListItem {
  domain.UserListItem(
    id:,
    email: id <> "@example.com",
    created: ts(created),
    updated: None,
    roles: [],
    last_login: option.map(last_login, ts),
  )
}
