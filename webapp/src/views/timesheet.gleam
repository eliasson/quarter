import domain/color
import gleam/list
import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, header}
import message
import model
import ui/graphics
import ui/input

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    timesheet_header(m),
    timesheet(),
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

fn timesheet() {
  let grid =
    [input.ghost_button(graphics.icon_extend_earlier, message.Noop)]
    |> list.append(timesheet_grid())
    |> list.append([
      input.ghost_button(graphics.icon_extend_later, message.Noop),
    ])

  // TODO Add list of activities
  // TOOD Add timesheet summary
  let context = [timesheet_summary()]

  div([att.class("timesheet-view")], [
    div([att.class("timesheet-grid")], grid),
    div([att.class("timesheet-context")], context),
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
          stub_activity_color_badge(color.Color(32, 142, 178)),
          html.text("Task One"),
        ]),
        div([att.class("summary-activity-total")], [html.text("15m")]),
      ]),
      div([att.class("summary-activity")], [
        div([att.class("summary-activity-name")], [
          stub_activity_color_badge(color.Color(232, 42, 178)),
          html.text("Another"),
        ]),
        div([att.class("summary-activity-total")], [html.text("3h 00m")]),
      ]),
    ]),
  ])
}

/// Return the visible range of hours for the timesheet.
fn timesheet_grid() {
  [
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
    div([att.class("grid-row")], [
      div([att.class("hour-label")], [html.text("08:00")]),

      div([att.class("quarters")], [
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
      ]),
    ]),

    div([att.class("grid-row")], [
      div([att.class("hour-label")], [html.text("09:00")]),

      div([att.class("quarters")], [
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
      ]),
    ]),

    div([att.class("grid-row")], [
      div([att.class("hour-label")], [html.text("10:00")]),

      div([att.class("quarters-container")], [
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
        div([att.class("quarter-cell")], []),
      ]),
    ]),
  ]
}

fn stub_activity_color_badge(c: color.Color) {
  let border_color = color.darken(c)

  div(
    [
      att.class("activity-color"),
      att.styles([
        #("background-color", color.color_to_style_value(c)),
        #("border-color", color.color_to_style_value(border_color)),
      ]),
    ],
    [],
  )
}
