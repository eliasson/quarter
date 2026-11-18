import gleam/uri.{type Uri}

/// The page routes for the admin application.
pub type Route {
  /// List and add system users.
  Users
}

pub const home_url = "/admin"

pub const users_url = "/admin/users"

/// The time tracking application. Served as a separate application so it must be loaded, not routed to.
pub const app_url = "/ui"

pub const logout_url = "/account/logout"

pub const login_url = "/ui/login.html"

pub fn identify(uri: Uri) -> Route {
  case uri.path_segments(uri.path) {
    ["admin", "users"] -> Users
    // The users view is the start page of the admin application.
    _ -> Users
  }
}

/// Get the URL for the given route.
pub fn to_url(route: Route) -> String {
  case route {
    Users -> users_url
  }
}
