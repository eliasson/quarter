import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message
import model
import modem
import route
import view

pub fn main() {
  let app = lustre.application(init, update, view.view)
  let assert Ok(_) = lustre.start(app, "#quarter", Nil)

  Nil
}

fn init(_args) -> #(model.Model, Effect(message.Msg)) {
  #(model.initial_model(), modem.init(on_url_change))
}

/// The orchestration of the application. All user actions, HTTP responses, etc. is dispatched
/// here as messages together with the current model.
/// Perform updates to the model and triggers optional HTTP requests, etc. as effects.
pub fn update(
  model: model.Model,
  msg: message.Msg,
) -> #(model.Model, Effect(message.Msg)) {
  case msg {
    message.OnRouteChange(r) -> #(model.navigate_to(model, r), effect.none())
    message.OpenMainMenu -> #(model.open_main_menu(model), effect.none())
  }
}

fn on_url_change(uri: Uri) -> message.Msg {
  route.identify(uri) |> message.OnRouteChange()
}
