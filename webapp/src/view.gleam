import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, li, nav, ul}
import message
import model
import ui

pub fn view(model: model.Model) -> Element(message.Msg) {
  div([], [main_navigation(model)])
}

fn main_navigation(_model: model.Model) -> Element(message.Msg) {
  nav([att.class("main-navigation")], [nav_logo(), ..nav_menu()])
}

fn nav_logo() {
  div([att.class("main-navigation-item")], [ui.icon("logo")])
}

fn nav_menu() {
  let item = fn(icon: String, label: String) {
    li([att.class("main-navigation-item")], [
      ui.icon(icon),
      html.a([att.href("#")], [html.text(label)]),
    ])
  }

  // For small screens, should this be a single menu?
  [
    ul([], [
      item("ellipsis-horizontal-circle", "Timesheet"),
      item("chart-bar", "Report"),
      item("table", "Manage"),
    ]),
    div([att.class("main-navigation-item")], [
      ui.icon("ellipsis-horizontal-circle"),
    ]),
  ]
}
