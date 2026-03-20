import gleam/time/timestamp
import gleam/uri.{type Uri}
import util/timestamp as tsutil

/// The page routes for the application.
pub type Route {
  // The home route displays the current month and the time registered per day.
  Home
  /// The timesheet defaults to today and is where time is registered.
  Timesheet(date: timestamp.Timestamp)
  /// Manage projects and activities
  Manage
  /// Reports for time registration.
  Report(date: timestamp.Timestamp)
  /// Admin view for managing users.
  AdministerSystemUsers
  /// Admin view for managing features.
  AdministerSystemFeatures
}

pub const home_url = "/ui"

pub const timesheet_url = "/ui/timesheet"

pub const manage_url = "/ui/manage"

pub const report_url = "/ui/report"

pub const logout_url = "/account/logout"

pub const admin_users_url = "/ui/admin/users"

pub const admin_features_url = "/ui/admin/features"

/// Describe the route in text.
pub fn describe(route: Route) -> String {
  case route {
    Home -> "Home"
    Timesheet(d) -> "Timesheet for " <> tsutil.to_iso_date(d)
    Manage -> "Manage"
    Report(d) -> "Report for " <> tsutil.to_iso_date(d)
    AdministerSystemUsers -> "Administrate system users"
    AdministerSystemFeatures -> "Administrate system features"
  }
}

pub fn identify(uri: Uri) -> Route {
  case uri.path_segments(uri.path) {
    ["ui"] -> Home

    ["ui", "timesheet"] ->
      Timesheet(tsutil.with_zero_time(timestamp.system_time()))

    ["ui", "timesheet", tail] -> {
      case tsutil.from_iso_date(tail) {
        Ok(ts) -> Timesheet(ts)
        _ -> Home
        // TODO Add a not found route
      }
    }

    ["ui", "report"] -> Report(tsutil.with_zero_time(timestamp.system_time()))

    ["ui", "report", tail] -> {
      case tsutil.from_iso_date(tail) {
        Ok(ts) -> Report(ts)
        _ -> Home
        // TODO Add a not found route
      }
    }

    ["ui", "manage"] -> Manage
    ["ui", "admin", "users"] -> AdministerSystemUsers
    ["ui", "admin", "features"] -> AdministerSystemFeatures
    _ -> Home
  }
}

/// Create the URL to the timesheet view for a specific date.
pub fn for_timesheet(timestamp: timestamp.Timestamp) -> String {
  timesheet_url <> "/" <> tsutil.to_iso_date(timestamp)
}

/// Create the URL to the report view for a specific date.
pub fn for_report(timestamp: timestamp.Timestamp) -> String {
  report_url <> "/" <> tsutil.to_iso_date(timestamp)
}

/// Get the URL for the given route.
pub fn to_url(route: Route) -> String {
  case route {
    Home -> home_url
    Timesheet(d) -> for_timesheet(d)
    Manage -> manage_url
    Report(d) -> for_report(d)
    AdministerSystemUsers -> admin_users_url
    AdministerSystemFeatures -> admin_features_url
  }
}

/// Check if the given route is active based on the current route.
pub fn is_active(route: Route, current: Route) -> Bool {
  case route, current {
    Timesheet(_), Timesheet(_) -> True
    _, _ -> route == current
  }
}
