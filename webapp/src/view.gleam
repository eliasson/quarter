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

const main_menu_id = "main.nav"

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
  div([att.class("main-navigation-item")], [
    ui.icon(gfx.icon_logo, ui.MediumSize),
  ])
}

fn nav_menu() {
  let item = fn(icon: String, label: String, path: String) {
    li([att.class("main-navigation-item")], [
      ui.icon(icon, ui.MediumSize),
      html.a([att.href(path)], [html.text(label)]),
    ])
  }

  ul([], [
    item(gfx.icon_timesheet, "Timesheet", route.timesheet_url),
    item(gfx.icon_report, "Report", route.report_url),
    item(gfx.icon_manage, "Manage", route.manage_url),
  ])
}

fn main_drop_down_menu(m: model.Model) {
  ui.drop_down_menu(
    main_menu_id,
    ui.icon(gfx.icon_menu, ui.MediumSize),
    [
      ui.DropDownHeader,
      ui.DropDownLinkApx(
        gfx.icon_timesheet,
        "Timesheet",
        "Register time per day",
        route.timesheet_url,
      ),
      ui.DropDownLinkApx(
        gfx.icon_report,
        "Report",
        "Generate and export reports",
        route.report_url,
      ),

      ui.DropDownLinkApx(
        gfx.icon_manage,
        "Manage",
        "Manage project and activities",
        route.manage_url,
      ),
      ui.DropDownSeparator,
      ui.DropDownLink(gfx.icon_features, "Features", route.admin_features_url),
      ui.DropDownLink(gfx.icon_users, "Users", route.admin_users_url),
      ui.DropDownSeparator,
      ui.DropDownLink(gfx.icon_logout, "Logout", route.logout_url),
    ],
    model.is_drop_down_menu_open(m, main_menu_id),
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
