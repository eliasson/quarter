import lustre
import lustre/effect.{type Effect}
import message
import model
import view

pub fn main() {
  let app = lustre.application(init, update, view.view)
  let assert Ok(_) = lustre.start(app, "#quarter", Nil)

  Nil
}

fn init(_args) -> #(model.Model, Effect(message.Msg)) {
  #(model.initial_model(), effect.none())
}

fn update(
  model: model.Model,
  msg: message.Msg,
) -> #(model.Model, Effect(message.Msg)) {
  case msg {
    message.Incr -> #(model, effect.none())
    message.Decr -> #(model, effect.none())
  }
}
