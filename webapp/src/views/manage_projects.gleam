import dialogs/activity_dialog
import dialogs/project_dialog
import domain/color
import domain/input_value
import domain/project
import form
import gleam/list
import gleam/option
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, span}
import lustre/event
import message
import model
import ui/activity.{activity_badge}
import ui/core as ui
import ui/dropdown
import ui/graphics
import ui/input

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    div([att.class("content-heading")], [
      h1([], [html.text("Projects & Activities")]),
      input.icon_button(
        "button",
        graphics.icon_plus,
        "New project",
        False,
        message.OpenDialog(model.new_project_dialog()),
      ),
    ]),
    ..project_list(m)
  ])
}

fn project_list(m: model.Model) {
  let projects = project.sort_projects(project.to_list(m.projects))

  list.map(projects, fn(project) {
    let is_expanded = model.is_project_expanded(m, project.id)

    let project_row_classes =
      [att.class("project-row")]
      |> cond_class(is_expanded, "expanded")
      |> cond_class(project.is_archived, "archived")

    let icon = case is_expanded {
      True -> graphics.icon_is_open
      False -> graphics.icon_is_closed
    }

    let project_archived_chip = case project.is_archived {
      True -> ui.chip("Archived")
      False -> element.none()
    }

    // Actions for this project, unless the project is archied.
    let project_actions = case project.is_archived {
      True -> []
      False -> [
        div([att.class("activity-row add-activity")], [
          input.icon_button(
            "button",
            graphics.icon_plus,
            "Add activity",
            False,
            message.OpenDialog(model.new_activity_dialog(project)),
          ),
        ]),
      ]
    }

    div(project_row_classes, [
      div(
        [
          att.class("project-info"),
          event.on_click(message.ToggleProject(project.id)),
        ],
        [
          div([att.class("name")], [html.text(project.name)]),
          div([att.class("state")], [project_archived_chip]),
          div([att.class("action")], [
            input.fake_button(icon),
          ]),
        ],
      ),
      div([att.class("project-details")], [
        span([], [html.text(project.description)]),
        manage_project_action(m, project),
      ]),
      div(
        [att.class("activities")],
        list.append(
          list.map(project.sort_activities(project.activities), fn(activity) {
            let activity_row_classes =
              [att.class("activity-row")]
              |> cond_class(activity.is_archived, "archived")

            let activity_archived_chip = case activity.is_archived {
              True -> ui.chip("Archived")
              False -> element.none()
            }

            div(activity_row_classes, [
              activity_badge(activity),
              div([att.class("name")], [html.text(activity.name)]),
              div([att.class("state")], [activity_archived_chip]),
              div([att.class("action")], [manage_activity_action(activity, m)]),
            ])
          }),
          project_actions,
        ),
      ),
    ])
  })
}

fn manage_project_action(m: model.Model, project: project.Project) {
  let menu_id = "project." <> project.id.value

  dropdown.drop_down_menu(
    menu_id,
    input.fake_button(graphics.icon_context_menu),
    [
      dropdown.DropDownMsg(
        graphics.icon_edit,
        "Edit project",
        message.OpenDialog(model.edit_project_dialog(project)),
      ),
      dropdown.DropDownMsg(
        graphics.icon_archive,
        archive_project_menu_label(project),
        message.ConfirmArchiveProject(project),
      ),
      dropdown.DropDownMsg(
        graphics.icon_delete,
        "Delete project",
        message.ConfirmDeleteProject(project),
      ),
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
    input.ghost_button(graphics.icon_context_menu, message.CloseModal),
    [
      dropdown.DropDownMsg(
        graphics.icon_edit,
        "Edit activity",
        message.OpenDialog(model.edit_activity_dialog(activity)),
      ),
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

pub fn add_project_form(state: project_dialog.State) -> form.Form {
  project_form(state, option.None)
}

pub fn edit_project_form(
  state: project_dialog.State,
  project: project.Project,
) -> form.Form {
  project_form(state, option.Some(project))
}

pub fn add_activity_form(
  state: activity_dialog.State,
  _project: project.Project,
) -> form.Form {
  activity_form(state, option.None)
}

pub fn edit_activity_form(
  state: activity_dialog.State,
  activity: project.Activity,
) -> form.Form {
  activity_form(state, option.Some(activity))
}

fn activity_form(
  state: activity_dialog.State,
  activity: option.Option(project.Activity),
) -> form.Form {
  let id = case activity {
    option.Some(_) -> "EditActivityDialog"
    option.None -> "AddActivityDialog"
  }

  let color_value = case state.color {
    input_value.ValidValue(c) -> color.to_hex(c)
    input_value.InvalidValue(c, _) -> color.to_hex(c)
    input_value.UnvalidatedValue(c) -> color.to_hex(c)
  }

  form.Form(
    id,
    [
      form.TextInput("name", "Activity name", state.name.value, True, True),
      form.TextAreaInput(
        "description",
        "Description",
        state.description.value,
        True,
        False,
      ),
      form.ColorInput("color", "Color", color_value, True, False),
    ],
    [
      form.Cancel,
      form.Confirm(!state.is_valid, message.ConfirmDialog),
    ],
  )
}

fn project_form(
  state: project_dialog.State,
  project: option.Option(project.Project),
) -> form.Form {
  let id = case project {
    option.Some(_) -> "AddProjectDialog"
    option.None -> "EditProjectDialog"
  }

  form.Form(
    id,
    [
      form.TextInput("name", "Project name", state.name.value, True, True),
      form.TextAreaInput(
        "description",
        "Description",
        state.description.value,
        True,
        False,
      ),
    ],
    [
      form.Cancel,
      form.Confirm(!state.is_valid, message.ConfirmDialog),
    ],
  )
}

/// The archive project confirmation dialog is stateless and only includes a query text message.
pub fn archive_project_form(
  project: project.Project,
) -> element.Element(message.Msg) {
  form.Form(
    "ArchiveProject",
    [
      form.TextMessage(archive_project_text(project)),
    ],
    [
      form.Cancel,
      form.Confirm(False, message.ArchiveProject(project)),
    ],
  )
  |> input.form_dialog(graphics.icon_add_user, archive_project_header(project))
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
  |> input.form_dialog(
    graphics.icon_add_user,
    archive_activity_header(activity),
  )
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
      form.Confirm(False, message.DeleteActivity(activity)),
    ],
  )
  |> input.form_dialog(graphics.icon_add_user, "Delete activity?")
}

pub fn delete_project_form(
  project: project.Project,
) -> element.Element(message.Msg) {
  form.Form(
    "DeleteProject",
    [
      form.TextMessage(
        "A deleted project and its activities can no longer be used to register time with and will no longer be available for existing timesheets. A deleted project cannot be restored! Do you want to delete the project "
        <> project.name
        <> "?",
      ),
    ],
    [
      form.Cancel,
      form.Confirm(False, message.DeleteProject(project)),
    ],
  )
  |> input.form_dialog(graphics.icon_add_user, "Delete project?")
}

fn archive_project_menu_label(project: project.Project) -> String {
  case project.is_archived {
    True -> "Unarchive project"
    False -> "Archive project"
  }
}

fn archive_activity_menu_label(activity: project.Activity) -> String {
  case activity.is_archived {
    True -> "Unarchive activity"
    False -> "Archive activity"
  }
}

/// Get the dialog header for archiving or unarchiving a project based on it's state.
fn archive_project_header(project: project.Project) -> String {
  case project.is_archived {
    True -> "Unarchive project?"
    False -> "Archive project?"
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

/// Get the text message for archiving or unarchiving a project based on it's state.
fn archive_project_text(project: project.Project) -> String {
  case project.is_archived {
    True ->
      "Do you want to unarchive the project "
      <> project.name
      <> "? This will enable you to register time with it again."
    False ->
      "An archived project cannot be used to register time with, but is still used for existing timesheets. An archived project can later be unarchived. Do you want to archive the project "
      <> project.name
      <> "?"
  }
}

/// Add conditonal classes to the list of attributes.
fn cond_class(classes: List(att.Attribute(a)), cond: Bool, class: String) {
  case cond {
    True -> list.append(classes, [att.class(class)])
    False -> classes
  }
}
