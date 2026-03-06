import domain/color
import domain/project
import gleam/int
import gleam/list
import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, header}
import message
import model
import ui/activity.{activitiy_color_badge, activity_badge}
import ui/graphics
import ui/input

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    timesheet_header(m),
    timesheet(m),
  ])
}

fn timesheet_header(m: model.Model) {
  let name_of_day = i18n.name_of_day(m.today, m.lang) |> i18n.capitalize
  let date = i18n.date_short(m.today, m.lang) |> i18n.capitalize()

  header([att.class("timesheet-header ")], [
    input.ghost_button(graphics.icon_prev, message.PreviousTimesheet),
    div([att.class("timesheet-header-content")], [
      h1([att.class("main")], [html.text(name_of_day)]),
      div([att.class("second")], [html.text(date)]),
    ]),
    input.ghost_button(graphics.icon_next, message.NextTimesheet),
  ])
}

fn timesheet(m: model.Model) {
  let grid =
    [input.ghost_button(graphics.icon_extend_earlier, message.Noop)]
    |> list.append(timesheet_grid())
    |> list.append([
      input.ghost_button(graphics.icon_extend_later, message.Noop),
    ])

  div([att.class("timesheet-view")], [
    div([att.class("timesheet-grid")], grid),
    div([att.class("timesheet-context")], [
      timesheet_summary(),
      activity_selection(m),
    ]),
  ])
}

/// Generate the summary of the timesheet where each project and activity is sumed.
fn timesheet_summary() {
  div([att.class("panel-section")], [
    div([att.class("panel-section-title")], [html.text("Summary")]),

    div([att.class("summary-list")], [
      div([att.class("summary-project")], [
        div([att.class("summary-project-name")], [html.text("Project Alpha")]),
        div([att.class("summary-project-total")], [html.text("3h 15m")]),
      ]),
      div([att.class("summary-activity")], [
        div([att.class("summary-activity-name")], [
          html.text("Task One"),
        ]),
        div([att.class("summary-activity-total")], [html.text("15m")]),
      ]),
      div([att.class("summary-activity")], [
        div([att.class("summary-activity-name")], [
          html.text("Another"),
        ]),
        div([att.class("summary-activity-total")], [html.text("3h 00m")]),
      ]),
      // Total
      div([att.class("summary-project total-row")], [
        div([att.class("summary-project-name")], [html.text("Total")]),
        div([att.class("summary-project-total")], [html.text("7h 30m")]),
      ]),
    ]),
  ])
}

fn activity_selection(m: model.Model) {
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

/// Return the visible range of hours for the timesheet.
fn timesheet_grid() {
  [
    div([att.class("grid-row")], [
      div([att.class("hour-label")], [html.text("06:00")]),

      div([att.class("quarters")], [
        div(
          [
            att.class("quarter-cell"),
            att.style("background-color", "#8b5cf6"),
          ],
          [],
        ),
        div(
          [
            att.class("quarter-cell"),
            att.style("background-color", "#8b5cf6"),
          ],
          [],
        ),
        div(
          [
            att.class("quarter-cell"),
            att.style("background-color", "#8b5cf6"),
          ],
          [],
        ),
        div(
          [
            att.class("quarter-cell"),
            att.style("background-color", "#8b5cf6"),
          ],
          [],
        ),
      ]),
    ]),

    div([att.class("grid-row")], [
      div([att.class("hour-label")], [html.text("07:00")]),
      div([att.class("quarters")], [
        div(
          [
            att.class("quarter-cell"),
            att.style("background-color", "#8b5cf6"),
          ],
          [],
        ),
        div(
          [
            att.class("quarter-cell"),
            att.style("background-color", "#8b5cf6"),
          ],
          [],
        ),
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
      ]),
    ]),
    ..list.map(list.range(8, 23), fn(i) {
      div([att.class("grid-row")], [
        div([att.class("hour-label")], [html.text(int.to_string(i) <> ":00")]),

        div([att.class("quarters")], [
          div([att.class("quarter-cell")], []),
          div([att.class("quarter-cell")], []),
          div([att.class("quarter-cell")], []),
          div([att.class("quarter-cell")], []),
        ]),
      ])
    })
  ]
}
