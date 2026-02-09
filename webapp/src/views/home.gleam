import gleam/int
import gleam/list
import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, p, span}
import message
import model

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([att.class("content")], [
    calendar(m),
  ])
}

fn calendar(m: model.Model) {
  let month = i18n.name_of_month(m.today, m.lang) |> i18n.capitalize
  let year = i18n.year(m.today, m.lang) |> i18n.describe

  let days =
    list.map(m.timesheets, fn(ts) {
      let day = i18n.day(ts.date, m.lang) |> i18n.describe
      let name_of_day = i18n.name_of_day(ts.date, m.lang) |> i18n.capitalize

      div([att.class("calendar-month-day")], [
        div([att.class("date")], [html.text(day)]),
        div([att.class("day")], [html.text(name_of_day)]),
        div([att.class("reported")], [
          span([att.class("text-value")], [html.text("7.75")]),
          span([att.class("text-unit")], [html.text("h")]),
        ]),
      ])
    })

  div([att.class("calendar-month")], [
    h1([], [html.text(month), span([att.class("year")], [html.text(year)])]),
    ..days
  ])
}
