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
import ui/core as ui
import ui/dropdown
import ui/form
import views/system_users

const main_menu_id = "main.nav"

pub fn view(model: model.Model) -> Element(message.Msg) {
  let children = [
    drop_down_back_drop(model),
    main_navigation(model),
    route_view(model),
    debug(model),
  ]

  div([att.class("application-layout ")], list.append(children, dialogs(model)))
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
  dropdown.drop_down_menu(
    main_menu_id,
    ui.icon(gfx.icon_menu, ui.MediumSize),
    [
      dropdown.DropDownLinkApx(
        gfx.icon_timesheet,
        "Timesheet",
        "Register time per day",
        route.timesheet_url,
      ),
      dropdown.DropDownLinkApx(
        gfx.icon_report,
        "Report",
        "Generate and export reports",
        route.report_url,
      ),

      dropdown.DropDownLinkApx(
        gfx.icon_manage,
        "Manage",
        "Manage project and activities",
        route.manage_url,
      ),
      dropdown.DropDownSeparator,
      dropdown.DropDownLink(
        gfx.icon_features,
        "Features",
        route.admin_features_url,
      ),
      dropdown.DropDownLink(gfx.icon_users, "Users", route.admin_users_url),
      dropdown.DropDownSeparator,
      dropdown.DropDownLink(gfx.icon_logout, "Logout", route.logout_url),
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

fn dialogs(model: model.Model) -> List(element.Element(message.Msg)) {
  let markup = fn(d: model.Dialog) {
    case d {
      model.AddUserDialog(state) -> {
        system_users.add_user_form(state)
        |> form.form_dialog(gfx.icon_add_user, "Add new user")
      }
      model.AnotherDialog(_state) -> {
        element.none()
      }
    }
  }

  list.map(model.dialogs, fn(dialog) { markup(dialog) })
}

fn debug(m: model.Model) {
  html.pre([], [
    html.text("Route: " <> route.describe(m.route)),
    html.br([]),
    html.text("Modal count: " <> int.to_string(list.length(m.dropdowns))),
  ])
}
