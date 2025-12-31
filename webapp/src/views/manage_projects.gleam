import gleam/list
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, p}
import lustre/event
import message
import model
import project
import ui/core as ui
import ui/dropdown
import ui/form
import ui/graphics
import util

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    div([att.class("content-heading")], [
      div([], [
        h1([], [html.text("Manage projects")]),
        p([], [
          html.text(
            "Project and activities are used to track time. Each project can have any number of activity and each activity has a color which makes it easier to distinquish how your day is distributed.",
          ),
        ]),
      ]),
      ui.toolbar([
        form.icon_button(
          "button",
          graphics.icon_plus,
          "New project",
          False,
          message.Noop,
        ),
      ]),
    ]),
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

    let project_archived_chip = case project.is_archived {
      True -> ui.chip("Archived")
      False -> element.none()
    }

    div(row_classes, [
      div(
        [
          att.class("project-info"),
          event.on_click(message.ToggleProject(project.id)),
        ],
        [
          div([att.class("name")], [html.text(project.name)]),
          div([att.class("state")], [project_archived_chip]),
          div([att.class("action")], [
            form.fake_button(icon),
          ]),
        ],
      ),
      div([att.class("project-details")], [
        html.text(project.description),
        manage_project_action(m, project.id),
      ]),
      div(
        [att.class("activities")],
        list.map(project.activities, fn(activity) {
          let activity_archived_chip = case activity.is_archived {
            True -> ui.chip("Archived")
            False -> element.none()
          }

          div([att.class("activity-row")], [
            color_badge(activity),
            div([att.class("name")], [html.text(activity.name)]),
            div([att.class("state")], [activity_archived_chip]),

            div([att.class("action")], [
              manage_activity_action(activity, m),
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

fn manage_project_action(m: model.Model, project_id: project.ProjectId) {
  let menu_id = "project." <> project_id.value

  dropdown.drop_down_menu(
    menu_id,
    form.fake_button(graphics.icon_context_menu),
    [
      dropdown.DropDownMsg(graphics.icon_edit, "Edit project", message.Noop),
      dropdown.DropDownMsg(
        graphics.icon_archive,
        "Archive project",
        message.Noop,
      ),
      dropdown.DropDownMsg(graphics.icon_delete, "Delete project", message.Noop),
    ],
    model.is_drop_down_menu_open(m, menu_id),
  )
}

fn manage_activity_action(
  activity: project.Activity,
  m: model.Model,
) -> element.Element(message.Msg) {
  // Each menu item needs a unique ID
  let menu_id = "activity." <> activity.id.value

  dropdown.drop_down_menu(
    menu_id,
    form.ghost_button(graphics.icon_context_menu, message.CloseModal),
    [
      dropdown.DropDownMsg(graphics.icon_edit, "Edit activity", message.Noop),
      dropdown.DropDownMsg(
        graphics.icon_archive,
        archive_activity_menu_label(activity),
        message.ConfirmArchiveActivity(activity),
      ),
      dropdown.DropDownMsg(
        graphics.icon_delete,
        "Delete activity",
        message.ConfirmDeleteActivity(activity),
      ),
    ],
    model.is_drop_down_menu_open(m, menu_id),
  )
}

pub fn add_project_form(state: model.ProjectDialogState) -> form.Form {
  form.Form("AddProjectDialog", [], [
    form.Cancel,
    form.Confirm(!state.is_valid, message.ConfirmDialog),
  ])
}

/// The archive activity confirmation dialog is stateless and only includes a query text message.
pub fn archive_activity_form(
  activity: project.Activity,
) -> element.Element(message.Msg) {
  form.Form(
    "ArchiveActivity",
    [
      form.TextMessage(archive_activity_text(activity)),
    ],
    [
      form.Cancel,
      form.Confirm(False, message.ArchiveActivity(activity)),
    ],
  )
  |> form.form_dialog(graphics.icon_add_user, archive_activity_header(activity))
}

/// The archive activity confirmation dialog is stateless and only includes a query text message.
pub fn delete_activity_form(
  activity: project.Activity,
) -> element.Element(message.Msg) {
  form.Form(
    "DeleteActivity",
    [
      form.TextMessage(
        "A deleted activity can no longer be used to register time with and will no longer be available for existing timesheets. A deleted activity cannot be restored! Do you want to delete the activity "
        <> activity.name
        <> "?",
      ),
    ],
    [
      form.Cancel,
      form.Confirm(False, message.ArchiveActivity(activity)),
    ],
  )
  |> form.form_dialog(graphics.icon_add_user, "Delete activity?")
}

fn archive_activity_menu_label(activity: project.Activity) -> String {
  case activity.is_archived {
    True -> "Unarchive activity"
    False -> "Archive activity"
  }
}

/// Get the dialog header for archiving or unarchiving an activity based on it's state.
fn archive_activity_header(activity: project.Activity) -> String {
  case activity.is_archived {
    True -> "Unarchive activity?"
    False -> "Archive activity?"
  }
}

/// Get the text message for archiving or unarchiving an activity based on it's state.
fn archive_activity_text(activity: project.Activity) -> String {
  case activity.is_archived {
    True ->
      "Do you want to unarchive the activity "
      <> activity.name
      <> "? This will enable you to register time with it again."
    False ->
      "An archived activity cannot be used to register time with, but is still used for existing timesheets. An archived activity can later be unarchived. Do you want to archive the activity "
      <> activity.name
      <> "?"
  }
}
