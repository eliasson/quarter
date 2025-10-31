import route.{type Route}

pub type Msg {
  /// When a page navigation takes place.
  OnRouteChange(Route)

  /// Open the main navigation menu.
  OpenMainMenu

  ///When the modal backdrop is clicked.
  BackdropClick
}
