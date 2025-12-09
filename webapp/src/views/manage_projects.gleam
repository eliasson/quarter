import gfx
import gleam/list
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, table, tbody, td, th, thead, tr}
import message
import model
import ui/core as ui
import ui/dropdown
import ui/form

const manage_menu_id = "manage.projects"

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    h1([], [html.text("Manage projects")]),
    ui.toolbar([manage_action(m)]),
    project_table(m),
  ])
}

fn project_table(m: model.Model) {
  div([att.class("table-wrapper")], [
    table([], [
      thead([], [
        tr([], [
          th([], [html.text("")]),
          th([], [html.text("Name")]),
          th([], [html.text("")]),
        ]),
      ]),
      tbody(
        [],
        list.map(m.projects, fn(p) {
          tr([], [
            td([], [form.checkbox()]),
            td([], [html.text(p.name)]),
            td([att.class("action")], []),
          ])
        }),
      ),
    ]),
  ])
}

fn manage_action(m: model.Model) {
  dropdown.drop_down_menu(
    manage_menu_id,
    form.outline_button("Manage", "chevron-down"),
    [
      dropdown.DropDownMsg(
        gfx.icon_add_user,
        "Add project",
        message.OpenDialog(model.new_user_dialog()),
      ),
    ],
    model.is_drop_down_menu_open(m, manage_menu_id),
  )
}

pub fn add_project_form(state: model.ProjectDialogState) -> form.Form {
  form.Form("AddProjectDialog", [], [
    form.Cancel,
    form.Confirm(!state.is_valid),
  ])
}
