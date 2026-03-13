import dialogs/user_dialog
import form
import gleam/list
import gleam/option.{None, Some}
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, table, tbody, td, th, thead, tr}
import message
import model
import ui/core as ui
import ui/graphics
import ui/input

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    div([att.class("content-heading")], [
      h1([], [html.text("Users")]),
      input.icon_button(
        "button",
        graphics.icon_add_user,
        "Add user",
        False,
        message.OpenDialog(model.new_user_dialog()),
      ),
    ]),
    user_table(m),
  ])
}

fn user_table(m: model.Model) {
  div([att.class("table-wrapper")], [
    table([], [
      thead([], [
        tr([], [
          th([], [html.text("E-mail")]),
          th([], [html.text("Joined")]),
          th([], [html.text("Updated")]),
          th([att.class("action")], [html.text("")]),
        ]),
      ]),
      tbody(
        [],
        list.map(m.users, fn(u) {
          tr([], [
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

/// Generate a form to render based on the dialog state.
pub fn add_user_form(state: user_dialog.State) -> form.Form {
  form.Form(
    "AddUserDialog",
    [form.EmailInput("email", "Email", state.email.value.value, True, True)],
    [
      form.Cancel,
      form.Confirm(!state.is_valid, message.ConfirmDialog),
    ],
  )
}
