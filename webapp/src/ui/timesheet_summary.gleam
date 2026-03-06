import lustre/attribute as att
import lustre/element/html.{div}

/// Generate the summary of the timesheet where each project and activity is sumed.
pub fn timesheet_summary() {
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
