import domain/color
import domain/project
import gleam/list
import gleam/option
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{button, div}
import lustre/event
import message
import model
import ui/activity.{activitiy_color_badge, activity_badge}
import ui/core as ui
import ui/graphics
import ui/markup

/// The activity list used in the timesheet view where the user can select the
/// currently active activity, or the "clear activities" activity.
pub fn timesheet_activities(m: model.Model) {
  div([att.class("panel-section")], [
    div([att.class("panel-section-title")], [html.text("Select activity")]),
    expander(m),
    activity_picker(m),
  ])
}

// Button used to display the current selected activity as well as to expand
// the activity list once toggled.
fn expander(m: model.Model) {
  let current_item = case m.selected_activity {
    option.Some(a) -> activity_item(m, a)
    option.None -> clear_activity_item(m)
  }

  let icon = case m.activity_picker_open {
    True -> graphics.icon_is_open
    False -> graphics.icon_is_closed
  }

  button(
    [
      att.class("activity-picker-toggler"),
      ui.click_stop(message.ToggleActivityPicker),
    ],
    [
      current_item,
      ui.icon(icon, ui.MediumSize),
    ],
  )
}

fn activity_picker(m: model.Model) {
  let classes =
    [att.class("activity-picker")]
    |> markup.cond_class(m.activity_picker_open, "open")

  let lines =
    project.active_projects(m.projects)
    |> list.map(fn(p) { project_items(m, p) })
    |> list.append([clear_activity_item(m)])

  div(classes, lines)
}

fn project_items(
  m: model.Model,
  project: project.Project,
) -> Element(message.Msg) {
  let activities = case list.is_empty(project.activities) {
    True -> [empty_project()]
    False -> list.map(project.activities, fn(a) { activity_item(m, a) })
  }

  div([att.class("picker-project-group")], [
    div([att.class("picker-project-title")], [html.text(project.name)]),
    ..activities
  ])
}

fn activity_item(m: model.Model, activity: project.Activity) {
  let classes = case m.selected_activity {
    option.Some(a) if activity.id == a.id -> [
      att.class("picker-item"),
      att.class("active"),
    ]
    _ -> [att.class("picker-item")]
  }

  let attributes =
    [event.on_click(message.SelectActivity(option.Some(activity)))]
    |> list.append(classes)

  div(attributes, [
    activity_badge(activity),
    div([att.class("picker-name")], [html.text(activity.name)]),
  ])
}

fn empty_project() {
  div([att.class("picker-empty-state")], [html.text("No activities")])
}

fn clear_activity_item(m: model.Model) -> Element(message.Msg) {
  let classes = case m.selected_activity {
    option.None -> [
      att.class("picker-item"),
      att.class("active"),
    ]
    _ -> [att.class("picker-item")]
  }

  let attributes =
    [event.on_click(message.SelectActivity(option.None))]
    |> list.append(classes)

  div(attributes, [
    activitiy_color_badge(color.Color(255, 255, 255)),
    div([att.class("picker-name")], [html.text("Clear activity")]),
  ])
}
