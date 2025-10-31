import gleam/uri.{type Uri}

/// The page routes for the application.
pub type Route {
  // The home route displays the current month and the time registered per day.
  Home
  /// The timesheet defaults to today and is where time is registered.
  Timesheet
  /// Manage projects and activities
  Manage
  /// Reports for time registration.
  Report
  /// Admin view for managing users.
  AdministerSystemUsers
  /// Admin view for managing features.
  AdministerSystemFeatures
}

/// Describe the route in text.
pub fn describe(route: Route) -> String {
  case route {
    Home -> "Home"
    Timesheet -> "Timesheet (today)"
    Manage -> "Manage"
    Report -> "Report"
    AdministerSystemUsers -> "Administrate system users"
    AdministerSystemFeatures -> "Administrate system features"
  }
}

pub fn identify(uri: Uri) -> Route {
  case uri.path_segments(uri.path) {
    ["ui"] -> Home
    ["ui", "timesheet"] -> Timesheet
    ["ui", "manage"] -> Manage
    ["ui", "report"] -> Report
    ["ui", "admin", "users"] -> AdministerSystemUsers
    ["ui", "admin", "features"] -> AdministerSystemFeatures
    _ -> Home
  }
}
