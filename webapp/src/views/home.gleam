import domain/duration
import gleam/int
import gleam/list
import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{a, div, h1, header, span}
import message
import model
import route
import ui/core as ui
import ui/graphics
import ui/input

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    calendar(m),
  ])
}

fn calendar(m: model.Model) {
  let days =
    list.map(m.timesheets, fn(ts) {
      let day = i18n.day(ts.date, m.lang) |> i18n.describe
      let name_of_day = i18n.name_of_day(ts.date, m.lang) |> i18n.capitalize
      let #(hours, minutes) = duration.to_hours_and_minutes(ts.duration)

      a(
        [
          att.class("calendar-month-day"),
          att.href(route.for_timesheet(ts.date)),
        ],
        [
          div([att.class("date")], [
            div([att.class("number")], [html.text(day)]),
          ]),
          div([att.class("name")], [html.text(name_of_day)]),
          div([att.class("time")], [
            span([att.class("text-value")], [html.text(int.to_string(hours))]),
            span([att.class("text-unit")], [html.text("h")]),
            span([att.class("text-value")], [
              html.text(int.to_string(minutes)),
            ]),
            span([att.class("text-unit")], [html.text("min")]),
          ]),
        ],
      )
    })

  div([att.class("calendar-month")], [calendar_header(m), ..days])
}

fn calendar_header(m: model.Model) {
  let month = i18n.name_of_month(m.today, m.lang) |> i18n.capitalize
  let year = i18n.year(m.today, m.lang) |> i18n.describe

  header([att.class("calendar-month-header")], [
    previous_month(),
    div([att.class("calendar-month-header-content")], [
      h1([att.class("month")], [html.text(month)]),
      div([att.class("year")], [html.text(year)]),
    ]),
    next_month(),
  ])
}

fn previous_month() {
  input.ghost_button(graphics.icon_prev, message.PreviousMonth)
}

fn next_month() {
  input.ghost_button(graphics.icon_next, message.NextMonth)
}
