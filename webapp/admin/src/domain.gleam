import gleam/option.{type Option, None, Some}
import gleam/order.{type Order}
import gleam/time/timestamp.{type Timestamp}
import quarter_shared/domain/user

/// The representation of a user that is used in the admin list of users.
///
/// It contains slightly more details than what is used in the app right now,
/// so it has has its own type. It's currently a super-set of the user.
pub type UserListItem {
  UserListItem(
    id: user.UserId,
    email: user.Email,
    created: Timestamp,
    updated: Option(Timestamp),
    roles: List(String),
    last_login: Option(Timestamp),
  )
}

/// Order users by activity, most recently seen first and then user who
/// has not logged in.
pub fn compare_by_activity(a: UserListItem, b: UserListItem) -> Order {
  case a.last_login, b.last_login {
    Some(x), Some(y) -> timestamp.compare(y, x)
    Some(_), None -> order.Lt
    None, Some(_) -> order.Gt
    None, None -> order.Eq
  }
  |> order.break_tie(timestamp.compare(b.created, a.created))
}
