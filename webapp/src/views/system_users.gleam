import gleam/list
import lustre/element.{type Element}
import lustre/element/html.{div}
import message
import model

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([], list_users(m))
}

fn list_users(m: model.Model) {
  list.map(m.users, fn(u) { div([], [html.text(u.email)]) })
}
