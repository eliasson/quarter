import gleam/option
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, header}
import message
import model
import ui/graphics
import ui/input

pub fn view(m: model.Model) -> Element(message.Msg) {
  case m.active_report {
    option.Some(_report) ->
      div([att.class("content")], [report_header(m), report(m)])
    _ -> div([att.class("content")], [html.text("Unable to load report")])
  }
}

fn report_header(m: model.Model) {
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

fn report(m: model.Model) {
  div([], [])
}
