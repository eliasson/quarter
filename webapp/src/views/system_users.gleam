import gfx
import gleam/list
import gleam/option.{None, Some}
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, table, tbody, td, th, thead, tr}
import lustre/event
import message
import model
import ui

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    h1([], [html.text("System users")]),
    ui.toolbar([
      ui.outline_button("Manage", "chevron-down"),
    ]),
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
            td([att.class("action")], []),
          ])
        }),
      ),
    ]),
  ])
}

/// The manage all users action. I.e. a view actoin
fn manage_action() {
  let menu =
    html.div([att.class("drop-down-menu")], [
      ui.drop_down_header(message.CloseModal),
      ui.drop_down_item_extended(
        "#TODO",
        gfx.icon_add_user,
        "Add user",
        "Add a new standard user.",
      ),
    ])

  div(
    [
      att.class("drop-down-initiator"),
      event.on_click(message.OpenDropDownMenu("TODO")),
    ],
    [ui.ghost_button(gfx.icon_context_menu), menu],
  )
}
