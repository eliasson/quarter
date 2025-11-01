import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message.{type Msg, CloseModal, OnRouteChange, OpenMainMenu}
import model.{
  type Model, close_modal, initial_model, navigate_to, open_main_menu,
}
import modem
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

  let m = initial_model() |> navigate_to(initial_route)
  #(m, modem.init(on_url_change))
}

/// The orchestration of the application. All user actions, HTTP responses, etc. is dispatched
/// here as messages together with the current model.
/// Perform updates to the model and triggers optional HTTP requests, etc. as effects.
pub fn update(model: Model, msg: Msg) -> #(Model, Effect(Msg)) {
  case msg {
    OnRouteChange(r) -> {
      let m = model |> close_modal |> navigate_to(r)
      #(m, effect.none())
    }
    OpenMainMenu -> #(open_main_menu(model), effect.none())
    CloseModal -> #(close_modal(model), effect.none())
  }
}

fn on_url_change(uri: Uri) -> Msg {
  route.identify(uri) |> OnRouteChange()
}
