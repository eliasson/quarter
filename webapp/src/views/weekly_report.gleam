import domain/duration
import domain/project
import domain/report
import gleam/int
import gleam/list
import gleam/option
import gleam/pair
import gleam/time/timestamp
import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{
  div, h1, header, table, tbody, td, tfoot, th, thead, tr,
}
import message
import model
import ui/activity.{activity_badge}
import ui/core as ui
import ui/graphics
import ui/input
import util/timestamp as tsutil

pub fn view(m: model.Model) -> Element(message.Msg) {
  case m.active_report {
    option.Some(report) ->
      div([att.class("content")], [
        report_header(report, m),
        render_report(report, m),
        report_footer(report, m),
      ])
    _ -> div([att.class("content")], [html.text("Unable to load report")])
  }
}

fn report_header(report: report.WeeklyReport, m: model.Model) {
  let period =
    i18n.date_medium(report.start_of_week, m.lang)
    |> i18n.capitalize()
    <> " — "
    <> i18n.date_medium(report.end_of_week, m.lang) |> i18n.capitalize()

  let week = i18n.week(report.start_of_week, m.lang) |> i18n.capitalize()

  header([att.class("report-header ")], [
    input.ghost_button(graphics.icon_prev, message.PreviousReportWeek),
    div([att.class("report-header-content")], [
      h1([att.class("main")], [html.text(week)]),
      div([att.class("second")], [html.text(period)]),
    ]),
    input.ghost_button(graphics.icon_next, message.NextReportWeek),
  ])
}

fn report_footer(report: report.WeeklyReport, m: model.Model) {
  div([att.class("report-footer")], [
    div([], [html.text("Total")]),
    div([], [
      html.text(i18n.duration(report.duration, m.lang) |> i18n.describe),
    ]),
  ])
}

fn render_report(report: report.WeeklyReport, m: model.Model) {
  case report.duration.value {
    0 ->
      ui.empty_state(
        "On holiday?",
        "There is no time registered for this week.",
      )
    _ -> report_table(report, m)
  }
}

fn report_table(report: report.WeeklyReport, m: model.Model) {
  // Generate a `tbody` section for each project and its activities
  let project_tables =
    list.map(report.usage, fn(u) { project_body(u, m.projects, m.lang) })

  let contents =
    [table_header(report.start_of_week, m.lang)]
    |> list.append(project_tables)
    |> list.append([table_footer(report.weekday_totals, m.lang)])

  div([att.class("table-wrapper")], [
    table([att.class("weekly-report-table")], contents),
  ])
}

fn table_header(timestamp: timestamp.Timestamp, lang: i18n.Language) {
  let columns =
    int.range(0, 7, [], list.prepend)
    |> list.map_fold(timestamp, fn(ts, _i) {
      #(
        // The accumulator is the next timestamp
        tsutil.tomorrow(ts),
        html.th([], [
          html.text(
            // Use the current timestamp to describe this column
            i18n.date_short(ts, lang)
            |> i18n.capitalize,
          ),
        ]),
      )
    })
    |> pair.second
    |> with_empty_header()

  thead([], [
    tr([], [th([], [html.text("")]), ..columns]),
  ])
}

fn table_footer(weekday_totals: List(duration.Duration), lang: i18n.Language) {
  let columns =
    weekday_totals
    |> list.map(fn(dur) {
      td([], [html.text(i18n.describe(i18n.as_hours_decimal(dur, lang)))])
    })
    |> with_empty_cell()

  tfoot([], [
    tr([], [td([], [html.text("Daily total")]), ..columns]),
  ])
}

fn project_body(
  usage: report.ProjectUsage,
  projects: project.ProjectCollection,
  lang: i18n.Language,
) {
  let activity_rows =
    list.map(usage.activity_usage, fn(au) {
      let columns =
        au.weekday_totals
        |> list.map(fn(dur) {
          case dur.value {
            0 -> no_value_cell()
            _ -> time_cell(dur, lang)
          }
        })
        |> with_time_total(au.duration, lang)

      case project.get_activity(projects, au.activity_id) {
        Ok(a) ->
          tr([], [
            td([], [
              div([att.class("activity")], [
                activity_badge(a, ui.SmallSize),
                html.text(a.name),
              ]),
            ]),
            ..columns
          ])
        _ -> tr([], [])
      }
    })

  case project.get_project(projects, usage.project_id) {
    Ok(p) -> {
      tbody([], [
        tr([], [
          th([], [html.text(p.name)]),
          th([att.colspan(8)], [html.text("")]),
        ]),
        ..activity_rows
      ])
    }

    _ -> tbody([], [])
  }
}

fn with_empty_header(elements: List(element.Element(a))) {
  list.append(elements, [th([], [])])
}

fn with_empty_cell(elements: List(element.Element(a))) {
  list.append(elements, [td([], [])])
}

fn no_value_cell() {
  td([], [html.text("—")])
}

fn with_time_total(
  elements: List(element.Element(a)),
  dur: duration.Duration,
  lang: i18n.Language,
) {
  list.append(elements, [
    td([att.class("total")], [
      html.text(i18n.describe(i18n.as_hours_decimal(dur, lang))),
    ]),
  ])
}

fn time_cell(dur: duration.Duration, lang: i18n.Language) {
  td([], [
    html.text(i18n.describe(i18n.as_hours_decimal(dur, lang))),
  ])
}
