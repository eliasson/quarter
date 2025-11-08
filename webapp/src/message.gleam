import model
import route.{type Route}
import rsvp
import user

pub type Msg {
  /// When a page navigation takes place.
  OnRouteChange(Route)

  /// Generic drop down menu that is keyed with an arbitrary key.
  OpenDropDownMenu(id: String)

  /// Opens the given dialog
  OpenDialog(dialog: model.Dialog)

  ///When the modal backdrop is clicked, menu closed, etc.
  CloseModal

  //
  // Protocol messages
  //
  CurrentUserResult(Result(user.User, rsvp.Error))
  SystemUsersResult(Result(List(user.User), rsvp.Error))
}
