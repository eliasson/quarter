import gleam/int
import lustre/element.{type Element, text}
import lustre/element/html.{button, div, p}
import lustre/event.{on_click}

import message
import model

pub fn view(model: model.Model) -> Element(message.Msg) {
  let count = int.to_string(model.counter)

  div([], [
    button([on_click(message.Incr)], [text(" + ")]),
    p([], [text(count)]),
    button([on_click(message.Decr)], [text(" - ")]),
  ])
}
