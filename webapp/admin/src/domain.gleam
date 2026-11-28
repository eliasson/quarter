import gleam/option.{type Option}
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
