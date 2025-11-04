import route.{type Route}
import rsvp
import user

pub type Msg {
  /// When a page navigation takes place.
  OnRouteChange(Route)

  /// Open the main navigation menu.
  OpenMainMenu

  ///When the modal backdrop is clicked, menu closed, etc.
  CloseModal

  //
  // Protocol messages
  //
  //
  CurrentUserResult(Result(user.CurrentUser, rsvp.Error))
}
