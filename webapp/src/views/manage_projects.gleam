import gleam/list
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div}
import lustre/event
import message
import model
import project
import ui/core as ui
import ui/dropdown
import ui/form
import ui/graphics
import util

const manage_menu_id = "manage.projects"

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("project-list")], [
    ui.toolbar([manage_action(m)]),
    ..project_list(m)
  ])
}

fn project_list(m: model.Model) {
  list.map(m.projects, fn(project) {
    let is_expanded = model.is_project_expanded(m, project.id)

    let row_classes = case is_expanded {
      True -> [att.class("project-row"), att.class("expanded")]
      False -> [att.class("project-row")]
    }

    let icon = case is_expanded {
      True -> graphics.icon_is_open
      False -> graphics.icon_is_closed
    }

    div(row_classes, [
      div(
        [
          att.class("project-info"),
          event.on_click(message.ToggleProject(project.id)),
        ],
        [
          div([att.class("name")], [html.text(project.name)]),
          div([att.class("state")], []),
          div([att.class("action")], [
            form.fake_button(icon),
          ]),
        ],
      ),
      div([att.class("project-description")], [html.text(project.description)]),
      div(
        [att.class("activities")],
        list.map(project.activities, fn(activity) {
          div([att.class("activity-row")], [
            color_badge(activity),
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

fn color_badge(activity: project.Activity) {
  let border_color = util.darken(activity.color)

  div(
    [
      att.class("activity-color"),
      att.styles([
        #("background-color", util.color_to_style_value(activity.color)),
        #("border-color", util.color_to_style_value(border_color)),
      ]),
    ],
    [],
  )
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
    form.ghost_button(graphics.icon_context_menu, message.CloseModal),
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
