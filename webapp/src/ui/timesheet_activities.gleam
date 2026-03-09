import domain/color
import domain/project
import gleam/list
import gleam/option
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div}
import lustre/event
import message
import model
import ui/activity.{activitiy_color_badge, activity_badge}

/// The activity list used in the timesheet view where the user can select the
/// currently active activity, or the "clear activities" activity.
pub fn timesheet_activities(m: model.Model) {
  let lines =
    project.to_list(m.projects)
    |> list.map(fn(p) { project_items(m, p) })
    |> list.append([clear_activity_item(m)])

  div([att.class("panel-section")], [
    div([att.class("panel-section-title")], [html.text("Select activity")]),
    div([att.class("activity-picker")], lines),
  ])
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
    option.Some(id) if activity.id == id -> [
      att.class("picker-item"),
      att.class("active"),
    ]
    _ -> [att.class("picker-item")]
  }

  let attributes =
    [event.on_click(message.SelectActivity(option.Some(activity.id)))]
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
