import gleam/list
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div}
import message
import model
import project
import ui/core as ui
import ui/dropdown
import ui/form
import ui/graphics

const manage_menu_id = "manage.projects"

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("project-list")], [
    ui.toolbar([manage_action(m)]),
    ..project_list(m)
  ])
}

fn project_list(m: model.Model) {
  list.map(m.projects, fn(project) {
    div([att.class("project-row")], [
      div([att.class("project-info")], [
        div([att.class("name")], [html.text(project.name)]),
        div([att.class("state")], []),
        div([att.class("action")], [
          form.ghost_button(graphics.icon_is_closed),
        ]),
      ]),
      div([att.class("project-description")], [html.text(project.description)]),
      div(
        [att.class("activities")],
        list.map(project.activities, fn(activity) {
          div([att.class("activity-row")], [
            div([att.class("activity-color")], []),
            div([att.class("title")], [html.text(activity.name)]),
            div([att.class("action")], [
              manage_activity_action(activity.id, m),
            ]),
          ])
        }),
      ),
    ])
  })
}

fn manage_action(m: model.Model) {
  dropdown.drop_down_menu(
    manage_menu_id,
    form.outline_button("Manage", "chevron-down"),
    [
      dropdown.DropDownMsg(
        graphics.icon_add_user,
        "Add project",
        message.OpenDialog(model.new_user_dialog()),
      ),
    ],
    model.is_drop_down_menu_open(m, manage_menu_id),
  )
}

fn manage_activity_action(activity_id: project.ActivityId, m: model.Model) {
  // Each menu item needs a unique ID
  let menu_id = "activity." <> activity_id.value
  dropdown.drop_down_menu(
    manage_menu_id,
    form.ghost_button(graphics.icon_context_menu),
    [
      dropdown.DropDownMsg(
        graphics.icon_add_user,
        "Edit activity",
        message.OpenDialog(model.new_user_dialog()),
      ),
    ],
    model.is_drop_down_menu_open(m, menu_id),
  )
}

pub fn add_project_form(state: model.ProjectDialogState) -> form.Form {
  form.Form("AddProjectDialog", [], [
    form.Cancel,
    form.Confirm(!state.is_valid),
  ])
}
