import gleam/int
import lustre
import lustre/effect.{type Effect}
import lustre/element.{text}
import lustre/element/html.{button, div, p}
import lustre/event.{on_click}

pub fn main() {
  let app = lustre.application(init, update, view)
  let assert Ok(_) = lustre.start(app, "#quarter", Nil)

  Nil
}

type Model {
  Model(counter: Int)
}

fn init(_args) -> #(Model, Effect(Msg)) {
  #(Model(0), effect.none())
}

type Msg {
  Incr
  Decr
}

fn update(model: Model, msg: Msg) -> #(Model, Effect(Msg)) {
  case msg {
    Incr -> #(Model(model.counter + 1), effect.none())
    Decr -> #(Model(model.counter + 1), effect.none())
  }
}

fn view(model: Model) {
  let count = int.to_string(model.counter)

  div([], [
    button([on_click(Incr)], [text(" + ")]),
    p([], [text(count)]),
    button([on_click(Decr)], [text(" - ")]),
  ])
}
