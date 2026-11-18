import gleam/list
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, li, nav, ul}
import lustre/event
import message
import model
import quarter_shared/ui/core as ui
import quarter_shared/ui/dropdown
import quarter_shared/ui/graphics
import quarter_shared/ui/input
import quarter_shared/ui/markup
import route
import views/system_users

const main_menu_id = "admin.nav"

pub fn view(model: model.Model) -> Element(message.Msg) {
  let children =
    [
      drop_down_back_drop(model),
      main_navigation(model),
      route_view(model),
    ]
    |> list.append(dialogs(model))
    |> list.append(error_notifications(model))

  div([att.class("application-layout")], children)
}

fn route_view(model: model.Model) {
  case model.route {
    route.Users -> system_users.view(model)
  }
}

fn main_navigation(model: model.Model) -> Element(message.Msg) {
  nav([att.class("main-navigation")], [
    nav_logo(),
    ui.spacer(),
    nav_menu(model),
    main_drop_down_menu(model),
  ])
}

fn nav_logo() {
  div([att.class("main-navigation-item")], [
    html.a([att.href(route.home_url), att.class("logo")], [
      ui.icon(graphics.icon_logo, ui.MediumSize),
      html.text("QUARTER ADMIN"),
    ]),
  ])
}

fn nav_menu(model: model.Model) {
  let item = fn(label: String, r: route.Route) {
    let classes =
      [att.class("main-navigation-item")]
      |> markup.cond_class(r == model.route, "active")

    li(classes, [html.a([att.href(route.to_url(r))], [html.text(label)])])
  }

  ul([], [item("Users", route.Users)])
}

fn main_drop_down_menu(m: model.Model) {
  dropdown.drop_down_menu(
    main_menu_id,
    ui.icon(graphics.icon_menu, ui.MediumSize),
    [
      // Not a link since the router would keep it within the admin application.
      dropdown.DropDownMsg(graphics.icon_logo, "Quarter", message.GoToApp),
      dropdown.DropDownSeparator,
      dropdown.DropDownMsg(graphics.icon_logout, "Logout", message.Logout),
    ],
    model.is_drop_down_menu_open(m, main_menu_id),
    on_open: message.OpenDropDownMenu,
    on_close: message.CloseModal,
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

fn dialogs(model: model.Model) -> List(Element(message.Msg)) {
  list.map(model.dialogs, fn(d) {
    case d {
      model.AddUserDialog(state) ->
        system_users.add_user_form(state)
        |> input.form_dialog(
          "Add new user",
          on_input: message.FormTextFieldUpdated,
        )
    }
  })
}

fn error_notifications(model: model.Model) -> List(Element(message.Msg)) {
  list.map(model.errors, fn(error) {
    ui.error_notification(
      error.id,
      "An error occured!",
      error.message,
      message.DismissError,
    )
  })
}
