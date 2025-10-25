import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, li, nav, ul}
import message
import model
import route
import ui

pub fn view(model: model.Model) -> Element(message.Msg) {
  div([], [main_navigation(model), debug(model)])
}

fn main_navigation(_model: model.Model) -> Element(message.Msg) {
  nav([att.class("main-navigation")], [nav_logo(), ..nav_menu()])
}

fn nav_logo() {
  div([att.class("main-navigation-item")], [ui.icon("logo")])
}

fn nav_menu() {
  let item = fn(icon: String, label: String, path: String) {
    li([att.class("main-navigation-item")], [
      ui.icon(icon),
      html.a([att.href(path)], [html.text(label)]),
    ])
  }

  // For small screens, should this be a single menu?
  [
    ul([], [
      item("ellipsis-horizontal-circle", "Timesheet", "/ui/timesheet"),
      item("chart-bar", "Report", "/ui/report"),
      item("table", "Manage", "/ui/manage"),
    ]),
    div([att.class("main-navigation-item")], [
      ui.icon("ellipsis-horizontal-circle"),
    ]),
  ]
}

fn debug(m: model.Model) {
  html.pre([], [html.text("Route: " <> route.describe(m.route))])
}
