import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, li, nav, ul}
import lustre/event
import message
import model
import route
import ui

pub fn view(model: model.Model) -> Element(message.Msg) {
  div([], [drop_down_back_drop(model), main_navigation(model), debug(model)])
}

fn main_navigation(model: model.Model) -> Element(message.Msg) {
  nav([att.class("main-navigation")], [
    nav_logo(),
    nav_menu(),
    main_drop_down_menu(model),
  ])
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

  ul([], [
    item("ellipsis-horizontal-circle", "Timesheet", "/ui/timesheet"),
    item("chart-bar", "Report", "/ui/report"),
    item("table", "Manage", "/ui/manage"),
  ])
}

fn main_drop_down_menu(model: model.Model) {
  // The menu if open.
  let menu = case model.dropdowns {
    [model.MainMenu] ->
      html.div([att.class("drop-down-menu")], [
        // A drop down menu item
        html.div([att.class("drop-down-menu-item")], [
          html.div([att.class("content")], [
            ui.icon("table"),
            html.a([att.href("#")], [html.text("Menu item A")]),
          ]),
        ]),
      ])
    _ -> element.none()
  }

  // The initiator is always visible.
  html.div(
    [
      att.class("drop-down-initiator"),
      event.on_click(message.OpenMainMenu),
    ],
    [
      ui.icon("ellipsis-horizontal-circle"),
      menu,
    ],
  )
}

/// Renders a full size element that consumes all space and is positioned behind any modal
/// (such as drop-downs, dialogs, etc) if open.
fn drop_down_back_drop(model: model.Model) {
  case model.dropdowns {
    [_] -> html.div([att.class("drop-down-menu-back-drop")], [])
    _ -> html.div([], [])
  }
}

fn debug(m: model.Model) {
  html.pre([], [html.text("Route: " <> route.describe(m.route))])
}
