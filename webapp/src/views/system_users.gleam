import gfx
import gleam/list
import gleam/option.{None, Some}
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, table, tbody, td, th, thead, tr}
import message
import model
import ui

const manage_menu_id = "manage.users"

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    h1([], [html.text("System users")]),
    ui.toolbar([manage_action(m)]),
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

fn manage_action(m: model.Model) {
  ui.drop_down_menu(
    manage_menu_id,
    ui.outline_button("Manage", "chevron-down"),
    [
      ui.DropDownLink(gfx.icon_add_user, "Add user", "#TODO"),
    ],
    model.is_drop_down_menu_open(m, manage_menu_id),
  )
}
