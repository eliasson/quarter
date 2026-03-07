import domain/duration
import domain/timesheet
import gleam/list
import i18n
import lustre/attribute as att
import lustre/element/html.{div}
import model

/// Generate the summary of the timesheet where each project and activity is sumed.
pub fn timesheet_summary(ts: timesheet.Timesheet, model: model.Model) {
  let summary = timesheet.summary(ts, model.projects)

  let list_items =
    summary.details
    |> list.flat_map(fn(pd) {
      let p_total =
        i18n.describe(i18n.duration(duration.Minutes(0), model.lang))

      let project_item =
        div([att.class("summary-project")], [
          div([att.class("summary-project-name")], [html.text(pd.name)]),
          div([att.class("summary-project-total")], [html.text(p_total)]),
        ])

      let activities =
        pd.activities
        |> list.map(fn(ad) {
          let dur = i18n.describe(i18n.duration(ad.duration, model.lang))

          div([att.class("summary-activity")], [
            div([att.class("summary-activity-name")], [html.text(ad.name)]),
            div([att.class("summary-activity-total")], [html.text(dur)]),
          ])
        })

      [project_item] |> list.append(activities)
    })

  let total_item =
    div([att.class("summary-project total-row")], [
      div([att.class("summary-project-name")], [html.text("Total")]),
      div([att.class("summary-project-total")], [
        html.text(i18n.describe(i18n.duration(summary.total, model.lang))),
      ]),
    ])

  let items = list_items |> list.append([total_item])

  div([att.class("panel-section")], [
    div([att.class("panel-section-title")], [html.text("Summary")]),
    div([att.class("summary-list")], items),
  ])
}
