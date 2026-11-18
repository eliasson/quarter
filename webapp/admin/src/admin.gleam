import gleam/time/calendar
import gleam/time/timestamp
import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message.{
  type Msg, AddUserResult, CloseModal, ConfirmDialog, CurrentUserResult,
  DismissError, FormTextFieldUpdated, GoToApp, Logout, OnRouteChange, OpenDialog,
  OpenDropDownMenu, SystemUsersResult,
}
import model.{type Model}
import modem
import protocol
import quarter_shared/protocol as shared_protocol
import route
import rsvp
import view

pub fn main() {
  let app = lustre.application(init, update, view.view)
  let assert Ok(_) = lustre.start(app, "#admin", Nil)

  Nil
}

fn init(_args) -> #(Model, Effect(Msg)) {
  // Get the inital URL to match the route to set in the model. This is when a deep-link or
  // page refresh occurs.
  let initial_route = case modem.initial_uri() {
    Ok(uri) -> route.identify(uri)
    _ -> route.Users
  }

  let m = model.initial_model() |> model.navigate_to(initial_route)

  let init_effects =
    effect.batch([
      // Setup the router to monitor for URL changes.
      modem.init(on_url_change),
      // Load the current user.
      shared_protocol.get_current_user(message.CurrentUserResult),
      // Load any additional data based on route.
      effect_on_route_loaded(initial_route),
    ])

  #(m, init_effects)
}

/// The orchestration of the admin application. All user actions, HTTP responses, etc. is
/// dispatched here as messages together with the current model.
pub fn update(m: Model, msg: Msg) -> #(Model, Effect(Msg)) {
  case msg {
    Logout -> load(m, route.logout_url)

    GoToApp -> load(m, route.app_url)

    OnRouteChange(r) -> {
      let updated =
        m
        |> model.close_all_modals
        |> model.navigate_to(r)

      #(updated, effect_on_route_loaded(r))
    }

    OpenDropDownMenu(id) -> model.open_drop_down_menu(m, id) |> no_effect()

    OpenDialog(d) -> model.open_dialog(m, d) |> no_effect()

    CloseModal -> model.close_modal(m) |> no_effect()

    ConfirmDialog -> handle_dialog_confirm(m)

    DismissError(id) -> model.dismiss_error(m, id) |> no_effect()

    FormTextFieldUpdated(value) ->
      model.update_dialog_value(m, value) |> no_effect()

    CurrentUserResult(Ok(u)) -> model.set_current_user(m, u) |> no_effect()

    SystemUsersResult(Ok(users)) -> model.set_users(m, users) |> no_effect()

    AddUserResult(Ok(_)) -> {
      // When the user was added successfully reload the list of users so the new user is
      // rendered. The only way to add a new user is when viewing the list of users.
      #(m, protocol.get_system_users(message.SystemUsersResult))
    }

    //
    // Error handling ----------------------------------------------------------------------------
    CurrentUserResult(Error(e)) ->
      m |> handle_error(e, "Unable to get current user.")

    SystemUsersResult(Error(e)) -> m |> handle_error(e, "Unable to get users.")

    AddUserResult(Error(e)) -> m |> handle_error(e, "Unable to add user.")
  }
}

fn on_url_change(uri: Uri) -> Msg {
  route.identify(uri) |> OnRouteChange()
}

/// Get the efffect (if any) to apply when loading a route.
fn effect_on_route_loaded(r: route.Route) -> Effect(Msg) {
  case r {
    route.Users -> protocol.get_system_users(message.SystemUsersResult)
  }
}

fn handle_dialog_confirm(m: Model) -> #(Model, Effect(Msg)) {
  let work = case model.current_dialog(m) {
    Ok(model.AddUserDialog(state)) ->
      protocol.add_user(state.email.value, message.AddUserResult)

    Error(_) -> effect.none()
  }

  #(model.close_modal(m), work)
}

/// Navigate away from the admin application with a full page load.
fn load(m: Model, url: String) -> #(Model, Effect(Msg)) {
  let assert Ok(uri) = uri.parse(url)
  #(m, modem.load(uri))
}

/// Returns a tuple of the model and no effect. Only a convenience function.
fn no_effect(m: Model) -> #(Model, Effect(Msg)) {
  #(m, effect.none())
}

/// Adds an error message to the model
fn with_error(m: Model, message: String) -> Model {
  // Generate a unique error ID so that errors can be dismissed.
  let id =
    timestamp.system_time()
    |> timestamp.to_rfc3339(calendar.utc_offset)

  model.add_error(m, model.ApplicationError(id:, message:))
}

/// Handles an HTTP error by redirecting to the login page if the error is an unauthorized error.
/// Else shows a generic error message.
fn handle_error(
  m: Model,
  error: rsvp.Error,
  fallback_msg: String,
) -> #(Model, Effect(Msg)) {
  case error {
    rsvp.HttpError(resp) if resp.status == 401 -> load(m, route.login_url)
    _ -> m |> with_error(fallback_msg) |> no_effect()
  }
}
