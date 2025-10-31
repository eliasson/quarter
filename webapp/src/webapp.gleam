import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message.{type Msg, BackdropClick, OnRouteChange, OpenMainMenu}
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
  #(initial_model(), modem.init(on_url_change))
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
    BackdropClick -> #(close_modal(model), effect.none())
  }
}

fn on_url_change(uri: Uri) -> Msg {
  route.identify(uri) |> OnRouteChange()
}
