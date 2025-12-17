import gleam/io
import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message.{
  type Msg, AddUserResult, CloseModal, ConfirmDialog, CurrentUserResult,
  DismissError, FormTextFieldUpdated, OnRouteChange, OpenDialog,
  OpenDropDownMenu, ProjectsResult, SystemUsersResult,
}
import model.{
  type Model, close_all_modals, close_modal, dismiss_error, initial_model,
  navigate_to, open_dialog, open_drop_down_menu, set_current_user, set_users,
  update_dialog_value,
}
import modem
import protocol
import route
import view

pub fn main() {
  let app = lustre.application(init, update, view.view)
  let assert Ok(_) = lustre.start(app, "#quarter", Nil)

  Nil
}

fn init(_args) -> #(Model, Effect(Msg)) {
  // Get the inital URL to match the route to set in the model. This is when a deep-link or
  // page refresh occurs.
  let initial_route = case modem.initial_uri() {
    Ok(uri) -> route.identify(uri)
    _ -> route.Home
  }

  let model = initial_model() |> navigate_to(initial_route)

  // At load we need to trigger two effects. One for setting up the URL routing and one to fetch
  // the current users to see if this user is authenticated or not.
  let init_effects =
    effect.batch([
      // Setup the router to monitor for URL changes.
      modem.init(on_url_change),
      // Load the current user.
      protocol.get_current_user(message.CurrentUserResult),
      // Get all available projects and activities.
      protocol.get_projects_and_activities(message.ProjectsResult),
      // Load any additional data based on route.
      effect_on_route_loaded(initial_route),
    ])

  #(model, init_effects)
}

/// The orchestration of the application. All user actions, HTTP responses, etc. is dispatched
/// here as messages together with the current model.
/// Perform updates to the model and triggers optional HTTP requests, etc. as effects.
pub fn update(model: Model, msg: Msg) -> #(Model, Effect(Msg)) {
  io.println("Update...")
  case msg {
    OnRouteChange(r) -> {
      io.println("OnRouteChange")
      let m = model |> close_all_modals |> navigate_to(r)
      let e = effect_on_route_loaded(r)

      #(m, e)
    }
    OpenDropDownMenu(id) -> {
      io.println("OpenDropDownMenu")
      #(open_drop_down_menu(model, id), effect.none())
    }
    OpenDialog(d) -> {
      io.println("OpenDialog")
      #(open_dialog(model, d), effect.none())
    }
    CloseModal -> {
      io.println("CloseModal")
      #(close_modal(model), effect.none())
    }
    ConfirmDialog -> handle_dialog_confirm(model)
    CurrentUserResult(Ok(u)) -> {
      io.println("CurrentUserResult OK")
      #(set_current_user(model, u), effect.none())
    }
    CurrentUserResult(Error(_)) -> {
      io.println("CurrentUserResult Error")
      #(model, effect.none())
    }
    SystemUsersResult(Ok(users)) -> {
      io.println("SystemUsersResult OK")
      #(set_users(model, users), effect.none())
    }
    SystemUsersResult(Error(_)) -> {
      io.println("SystemUsersResult Error")
      #(model, effect.none())
    }
    AddUserResult(Ok(_)) -> {
      io.println("AddUserResult OK")

      // When the user was added successfully add the new user to the state so it
      // can be rendered in the list of users. The only way to add a new user is
      // when viewing the list of users.
      #(model, protocol.get_system_users(message.SystemUsersResult))
    }
    AddUserResult(Error(_)) -> {
      io.println("AddUserResult Error")
      #(model, effect.none())
    }
    ProjectsResult(Ok(projects)) -> {
      io.println("ProjectsResult OK")
      #(model.Model(..model, projects:), effect.none())
    }
    ProjectsResult(Error(_)) -> {
      io.println("ProjectsResult Error")
      #(model, effect.none())
    }
    FormTextFieldUpdated(value) -> {
      io.println("FormTextFieldUpdated")
      #(update_dialog_value(model, value), effect.none())
    }
    DismissError(id) -> {
      io.println("DismissError")
      #(dismiss_error(model, id), effect.none())
    }
  }
}

fn on_url_change(uri: Uri) -> Msg {
  route.identify(uri) |> OnRouteChange()
}

/// Get the efffect (if any) to apply when loading a route.
fn effect_on_route_loaded(r: route.Route) {
  case r {
    route.AdministerSystemUsers ->
      protocol.get_system_users(message.SystemUsersResult)
    _ -> effect.none()
  }
}

fn handle_dialog_confirm(m: model.Model) {
  let work = case model.current_dialog(m) {
    Ok(model.AddUserDialog(state)) -> {
      protocol.add_user(state.email.value, message.AddUserResult)
    }
    _ -> effect.none()
  }

  #(close_modal(m), work)
}
