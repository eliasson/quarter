pub type UserId =
  String

pub type Email =
  String

pub type CurrentUser {
  CurrentUser(id: UserId, email: Email)
}
