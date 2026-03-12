import domain/color
import domain/timesheet
import gleam/int
import gleam/list
import gleam/option
import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{button, div, h1, header}
import message
import model
import ui/core as ui
import ui/graphics
import ui/input
import ui/timesheet_activities.{timesheet_activities}
import ui/timesheet_summary.{timesheet_summary}

pub fn view(m: model.Model) -> Element(message.Msg) {
  // Get the timesheet based on the current date
  // TODO: Produce a better error message in case of failure to get the timesheet.
  case m.active_timesheet {
    option.Some(ts) ->
      div([att.class("content")], [
        timesheet_header(m),
        timesheet(ts, m),
      ])
    _ -> div([att.class("content")], [html.text("Unable to load timesheet")])
  }
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

fn timesheet(timesheet: timesheet.Timesheet, m: model.Model) {
  let grid =
    [extend_start_of_day_button(m.start_of_day == 0)]
    |> list.append(timesheet_grid(timesheet, m))
    |> list.append([
      extend_end_of_day_button(m.start_of_day == 23),
    ])

  div([att.class("timesheet-view")], [
    div([att.class("timesheet-grid")], grid),
    div([att.class("timesheet-context")], [
      timesheet_summary(timesheet, m),
      timesheet_activities(m),
    ]),
  ])
}

/// Return the visible range of hours for the timesheet.
fn timesheet_grid(ts: timesheet.Timesheet, m: model.Model) {
  let hours =
    timesheet.hours(ts, m.start_of_day, m.end_of_day, m.projects)
    |> list.map(fn(h) {
      div([att.class("grid-row")], [
        div([att.class("hour-label")], [
          html.text(int.to_string(h.hour) <> ":00"),
        ]),
        div([att.class("quarters")], [
          cell(h.q1),
          cell(h.q2),
          cell(h.q3),
          cell(h.q4),
        ]),
      ])
    })

  hours
}

fn cell(c: option.Option(timesheet.QuarterDetail)) -> Element(message.Msg) {
  div([att.class("quarter-cell")], [])
  case c {
    option.Some(q) ->
      div(
        [
          att.class("quarter-cell"),
          att.style(
            "background-color",
            color.color_to_style_value(q.activity.color),
          ),
          ui.down_stop(message.StartRegistering),
        ],
        [],
      )
    option.None ->
      div(
        [
          att.class("quarter-cell"),
          ui.down_stop(message.StartRegistering),
        ],
        [],
      )
  }
}

fn extend_start_of_day_button(disabled: Bool) {
  button(
    [
      att.class("extend-button"),
      att.type_("button"),
      att.disabled(disabled),
      ui.click_stop(message.ExtendStartOfDay),
    ],
    [html.text("Earlier")],
  )
}

fn extend_end_of_day_button(disabled: Bool) {
  button(
    [
      att.class("extend-button"),
      att.type_("button"),
      att.disabled(disabled),
      ui.click_stop(message.ExtendEndOfDay),
    ],
    [html.text("Later")],
  )
}
