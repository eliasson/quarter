import domain/color
import domain/project
import gleam/list
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div}
import message
import model
import ui/activity.{activitiy_color_badge, activity_badge}

/// The activity list used in the timesheet view where the user can select the
/// currently active activity, or the "clear activities" activity.
pub fn timesheet_activities(m: model.Model) {
  let lines =
    m.projects
    |> list.map(project_items)
    |> list.append([clear_activity_item()])

  div([att.class("panel-section")], [
    div([att.class("panel-section-title")], [html.text("Select activity")]),
    div([att.class("activity-picker")], lines),
  ])
}

fn project_items(project: project.Project) -> Element(message.Msg) {
  let activities = case list.is_empty(project.activities) {
    True -> [empty_project()]
    False -> list.map(project.activities, activity_item)
  }

  div([att.class("picker-project-group")], [
    div([att.class("picker-project-title")], [html.text(project.name)]),
    ..activities
  ])
}

fn activity_item(activity: project.Activity) {
  div([att.class("picker-item")], [
    activity_badge(activity),
    div([att.class("picker-name")], [html.text(activity.name)]),
  ])
}

fn empty_project() {
  div([att.class("picker-empty-state")], [html.text("No activities")])
}

fn clear_activity_item() -> Element(message.Msg) {
  div([att.class("picker-item")], [
    activitiy_color_badge(color.Color(255, 255, 255)),
    div([att.class("picker-name")], [html.text("Clear activity")]),
  ])
}
