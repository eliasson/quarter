import route.{type Route}

pub type Msg {
  OnRouteChange(Route)

  /// Open the main navigation menu
  OpenMainMenu

  ///When the modal backdrop is clicked.
  BackdropClick
}
