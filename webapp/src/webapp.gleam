import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message.{
  type Msg, CloseModal, CurrentUserResult, OnRouteChange, OpenMainMenu,
  SystemUsersResult,
}
import model.{
  type Model, close_modal, initial_model, navigate_to, open_main_menu,
  set_current_user, set_users,
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
      modem.init(on_url_change),
      protocol.get_current_user(message.CurrentUserResult),
      effect_on_route_loaded(initial_route),
    ])

  #(model, init_effects)
}

/// The orchestration of the application. All user actions, HTTP responses, etc. is dispatched
/// here as messages together with the current model.
/// Perform updates to the model and triggers optional HTTP requests, etc. as effects.
pub fn update(model: Model, msg: Msg) -> #(Model, Effect(Msg)) {
  case msg {
    OnRouteChange(r) -> {
      let m = model |> close_modal |> navigate_to(r)
      let e = effect_on_route_loaded(r)

      #(m, e)
    }
    OpenMainMenu -> #(open_main_menu(model), effect.none())
    CloseModal -> #(close_modal(model), effect.none())
    CurrentUserResult(Ok(u)) -> #(set_current_user(model, u), effect.none())
    CurrentUserResult(Error(_)) -> #(model, effect.none())
    SystemUsersResult(Ok(users)) -> #(set_users(model, users), effect.none())
    SystemUsersResult(Error(_)) -> #(model, effect.none())
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
