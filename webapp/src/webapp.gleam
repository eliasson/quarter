import gleam/int
import lustre
import lustre/effect.{type Effect}
import lustre/element.{text}
import lustre/element/html.{button, div, p}
import lustre/event.{on_click}
import message

pub fn main() {
  let app = lustre.application(init, update, view)
  let assert Ok(_) = lustre.start(app, "#quarter", Nil)

  Nil
}

type Model {
  Model(counter: Int)
}

fn init(_args) -> #(Model, Effect(message.Msg)) {
  #(Model(0), effect.none())
}

fn update(model: Model, msg: message.Msg) -> #(Model, Effect(message.Msg)) {
  case msg {
    message.Incr -> #(Model(model.counter + 1), effect.none())
    message.Decr -> #(Model(model.counter + 1), effect.none())
  }
}

fn view(model: Model) {
  let count = int.to_string(model.counter)

  div([], [
    button([on_click(message.Incr)], [text(" + ")]),
    p([], [text(count)]),
    button([on_click(message.Decr)], [text(" - ")]),
  ])
}
