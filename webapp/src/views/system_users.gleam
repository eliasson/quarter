import gleam/list
import gleam/option.{None, Some}
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, table, tbody, td, th, thead, tr}
import message
import model
import ui

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    h1([], [html.text("System users")]),
    user_table(m),
  ])
}

fn user_table(m: model.Model) {
  div([att.class("table-wrapper")], [
    table([], [
      thead([], [
        tr([], [
          th([], [html.text("")]),
          th([], [html.text("E-mail")]),
          th([], [html.text("Joined")]),
          th([], [html.text("Updated")]),
          th([], [html.text("")]),
        ]),
      ]),
      tbody(
        [],
        list.map(m.users, fn(u) {
          tr([], [
            td([], [ui.checkbox()]),
            td([], [html.text(u.email)]),
            td([], [ui.timestamp(u.created)]),
            td([], [
              case u.updated {
                Some(ts) -> ui.timestamp(ts)
                None -> element.none()
              },
            ]),
            td([], []),
          ])
        }),
      ),
    ]),
  ])
}
