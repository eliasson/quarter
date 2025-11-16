import gfx
import gleam/list
import gleam/option.{None, Some}
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, table, tbody, td, th, thead, tr}
import message
import model
import ui/core as ui
import ui/dropdown
import ui/form
import util

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
            td([], [form.checkbox()]),
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
  let add_user_dialog =
    model.AddUserDialog(model.UserDialogState(util.Email("")))

  dropdown.drop_down_menu(
    manage_menu_id,
    form.outline_button("Manage", "chevron-down"),
    [
      dropdown.DropDownMsg(
        gfx.icon_add_user,
        "Add user",
        message.OpenDialog(add_user_dialog),
      ),
    ],
    model.is_drop_down_menu_open(m, manage_menu_id),
  )
}

/// Generate a form to render based on the dialog state.
pub fn add_user_form(d: model.UserDialogState) -> form.Form {
  // TODO Check if the form is invalid, add message and disable the confirm action if so.
  form.Form("add_user", [form.EmailInput("email", "Email", d.email.value)], [
    form.Cancel,
    form.Confirm,
  ])
}
