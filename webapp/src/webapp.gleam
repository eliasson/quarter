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

fn update(
  model: model.Model,
  msg: message.Msg,
) -> #(model.Model, Effect(message.Msg)) {
  case msg {
    message.OnRouteChange(r) -> #(model.navigate_to(model, r), effect.none())
  }
}

fn on_url_change(uri: Uri) -> message.Msg {
  route.identify(uri) |> message.OnRouteChange()
}
