import gfx
import gleam/int
import gleam/list
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, li, nav, ul}
import lustre/event
import message
import model
import route
import ui
import views/system_users

pub fn view(model: model.Model) -> Element(message.Msg) {
  div([att.class("application-layout ")], [
    drop_down_back_drop(model),
    main_navigation(model),
    route_view(model),
    debug(model),
  ])
}

fn route_view(model: model.Model) {
  case model.route {
    route.AdministerSystemUsers -> system_users.view(model)
    _ -> element.none()
  }
}

fn main_navigation(model: model.Model) -> Element(message.Msg) {
  nav([att.class("main-navigation")], [
    nav_logo(),
    nav_menu(),
    main_drop_down_menu(model),
  ])
}

fn nav_logo() {
  div([att.class("main-navigation-item")], [ui.icon(gfx.icon_logo)])
}

fn nav_menu() {
  let item = fn(icon: String, label: String, path: String) {
    li([att.class("main-navigation-item")], [
      ui.icon(icon),
      html.a([att.href(path)], [html.text(label)]),
    ])
  }

  ul([], [
    item(gfx.icon_timesheet, "Timesheet", route.timesheet_url),
    item(gfx.icon_report, "Report", route.report_url),
    item(gfx.icon_manage, "Manage", route.manage_url),
  ])
}

fn main_drop_down_menu(model: model.Model) {
  // The menu if open.
  let menu = case model.dropdowns {
    [model.MainMenu] ->
      html.div([att.class("drop-down-menu")], [
        ui.drop_down_header(message.CloseModal),
        ui.drop_down_item_extended(
          route.timesheet_url,
          gfx.icon_timesheet,
          "Timesheet",
          "Register time per day",
        ),
        ui.drop_down_item_extended(
          route.report_url,
          gfx.icon_report,
          "Report",
          "Generate and export reports",
        ),
        ui.drop_down_item_extended(
          route.manage_url,
          gfx.icon_manage,
          "Manage",
          "Manage project and activities",
        ),
        ui.separator_menu_item(),
        ui.drop_down_item(route.admin_users_url, gfx.icon_users, "Users"),
        ui.drop_down_item(
          route.admin_features_url,
          gfx.icon_features,
          "Features",
        ),
        ui.separator_menu_item(),
        ui.drop_down_item(route.logout_url, gfx.icon_logout, "Logout"),
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
      ui.icon(gfx.icon_menu),
      menu,
    ],
  )
}

/// Renders a full size element that consumes all space and is positioned behind any modal
/// (such as drop-downs, dialogs, etc) if open.
fn drop_down_back_drop(model: model.Model) {
  case model.dropdowns {
    [_] ->
      html.div(
        [
          att.class("drop-down-menu-back-drop"),
          event.on_click(message.CloseModal),
        ],
        [],
      )
    _ -> html.div([], [])
  }
}

fn debug(m: model.Model) {
  html.pre([], [
    html.text("Route: " <> route.describe(m.route)),
    html.br([]),
    html.text("Modal count: " <> int.to_string(list.length(m.dropdowns))),
  ])
}
