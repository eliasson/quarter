import gleam/uri.{type Uri}

/// The page routes for the application.
pub type Route {
  // The home route displays the current month and the time registered per day.
  Home
  /// The timesheet defaults to today and is where time is registered.
  Timesheet
}

/// Describe the route in text.
pub fn describe(route: Route) -> String {
  case route {
    Home -> "Home"
    Timesheet -> "Timesheet (today)"
  }
}

pub fn identify(uri: Uri) -> Route {
  case uri.path_segments(uri.path) {
    ["ui"] -> Home
    ["ui", "timesheet"] -> Timesheet
    _ -> Home
  }
}
