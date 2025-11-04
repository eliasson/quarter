pub type UserId =
  String

pub type Email =
  String

pub type User {
  User(id: UserId, email: Email)
}
