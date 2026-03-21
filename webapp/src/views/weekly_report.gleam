import domain/duration
import domain/report
import gleam/list
import gleam/option
import gleam/time/timestamp
import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{
  div, h1, header, table, tbody, td, tfoot, th, thead, tr,
}
import message
import model
import ui/core as ui
import ui/graphics
import ui/input

pub fn view(m: model.Model) -> Element(message.Msg) {
  case m.active_report {
    option.Some(report) ->
      div([att.class("content")], [
        report_header(m),
        render_report(report, m.lang),
      ])
    _ -> div([att.class("content")], [html.text("Unable to load report")])
  }
}

fn report_header(_m: model.Model) {
  header([att.class("report-header ")], [
    input.ghost_button(graphics.icon_prev, message.PreviousReportWeek),
    div([att.class("report-header-content")], [
      h1([att.class("main")], [
        div([], [html.text("16 Mar — 22 Mar")]),
        div([], [html.text("42.5h")]),
      ]),
      div([att.class("second")], [
        div([], [html.text("Week 12")]),
        div([], [html.text("Total")]),
      ]),
    ]),
    input.ghost_button(graphics.icon_next, message.NextReportWeek),
  ])
}

fn render_report(report: report.WeeklyReport, lang: i18n.Language) {
  case report.duration.value {
    0 ->
      ui.empty_state(
        "On holiday?",
        "There is no time registered for this week.",
      )
    _ -> report_table(report, lang)
  }
}

fn report_table(report: report.WeeklyReport, lang: i18n.Language) {
  let body =
    tbody([], [
      tr([], [
        th([], [html.text("Project Alpha")]),
        th([att.colspan(7)], [html.text("")]),
      ]),
      tr([], [
        td([], [html.text("Something something")]),
        td([], [html.text("3")]),
        td([], [html.text("—")]),
        td([], [html.text("—")]),
        td([], [html.text("—")]),
        td([], [html.text("—")]),
        td([], [html.text("—")]),
        td([], [html.text("—")]),
      ]),
    ])

  div([att.class("table-wrapper")], [
    table([att.class("weekly-report-table")], [
      table_header(report.start_of_week, lang),
      body,
      table_footer(report.weekday_totals, lang),
    ]),
  ])
}

fn table_header(timestamp: timestamp.Timestamp, lang: i18n.Language) {
  thead([], [
    tr([], [
      th([], [html.text("")]),
      th([], [html.text("Mon 16")]),
      th([], [html.text("Tue 17")]),
      th([], [html.text("Wed 18")]),
      th([], [html.text("Thu 19")]),
      th([], [html.text("Fri 20")]),
      th([], [html.text("Sat 21")]),
      th([], [html.text("Sun 22")]),
    ]),
  ])
}

fn table_footer(weekday_totals: List(duration.Duration), lang: i18n.Language) {
  let columns =
    weekday_totals
    |> list.map(fn(dur) {
      td([], [html.text(i18n.describe(i18n.as_hours_decimal(dur, lang)))])
    })

  tfoot([], [
    tr([], [td([], [html.text("Daily total")]), ..columns]),
  ])
}
