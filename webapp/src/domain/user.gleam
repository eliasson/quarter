import gleam/option
import gleam/time/timestamp.{type Timestamp}

pub type UserId =
  String

pub type Email =
  String

pub type User {
  User(
    id: UserId,
    email: Email,
    created: Timestamp,
    updated: option.Option(Timestamp),
  )
}
