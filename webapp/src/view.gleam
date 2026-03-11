import gleam/list
import gleam/time/timestamp
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, li, nav, ul}
import lustre/event
import message
import model
import route
import ui/core as ui
import ui/dropdown
import ui/graphics
import ui/input
import ui/markup
import views/home
import views/manage_projects
import views/system_users
import views/timesheet

const main_menu_id = "main.nav"

pub fn view(model: model.Model) -> Element(message.Msg) {
  let children = [
    drop_down_back_drop(model),
    main_navigation(model),
    route_view(model),
  ]

  div([att.class("application-layout")], list.append(children, dialogs(model)))
}

fn route_view(model: model.Model) {
  case model.route {
    route.Home -> home.view(model)
    route.Timesheet(_) -> timesheet.view(model)
    route.AdministerSystemUsers -> system_users.view(model)
    route.Manage -> manage_projects.view(model)
    _ -> element.none()
  }
}

fn main_navigation(model: model.Model) -> Element(message.Msg) {
  nav([att.class("main-navigation")], [
    nav_logo(),
    spacer(),
    nav_menu(model),
    main_drop_down_menu(model),
  ])
}

fn nav_logo() {
  div([att.class("main-navigation-item")], [
    html.a([att.href(route.home_url), att.class("logo")], [
      ui.icon(graphics.icon_logo, ui.MediumSize),
      html.text("QUARTER"),
    ]),
  ])
}

fn nav_menu(model: model.Model) {
  let item = fn(label: String, route: route.Route) {
    let is_active = route.is_active(route, model.route)

    let classes =
      [att.class("main-navigation-item")]
      |> markup.cond_class(is_active, "active")

    li(classes, [
      html.a([att.href(route.to_url(route))], [html.text(label)]),
    ])
  }

  ul([], [
    item("Calendar", route.Home),
    item("Timesheet", route.Timesheet(timestamp.system_time())),
    item("Report", route.Report),
    item("Manage", route.Manage),
  ])
}

fn main_drop_down_menu(m: model.Model) {
  dropdown.drop_down_menu(
    main_menu_id,
    ui.icon(graphics.icon_menu, ui.MediumSize),
    [
      dropdown.DropDownLinkApx(
        graphics.icon_calendar,
        "Calendar",
        "Montly overview",
        route.home_url,
      ),
      dropdown.DropDownLinkApx(
        graphics.icon_timesheet,
        "Timesheet",
        "Register time per day",
        route.timesheet_url,
      ),
      dropdown.DropDownLinkApx(
        graphics.icon_report,
        "Report",
        "Generate and export reports",
        route.report_url,
      ),

      dropdown.DropDownLinkApx(
        graphics.icon_manage,
        "Manage",
        "Manage project and activities",
        route.manage_url,
      ),
      dropdown.DropDownSeparator,
      dropdown.DropDownSeparator,
      dropdown.DropDownLink(graphics.icon_logout, "Logout", route.logout_url),
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
        |> input.form_dialog("Add new user")
      }

      model.AddProjectDialog(state) -> {
        manage_projects.add_project_form(state)
        |> input.form_dialog("Add new project")
      }

      model.EditProjectDialog(state, project) -> {
        manage_projects.edit_project_form(state, project)
        |> input.form_dialog("Edit " <> project.name)
      }

      model.AddActivityDialog(state, project) -> {
        manage_projects.add_activity_form(state, project)
        |> input.form_dialog("Add activity to " <> project.name)
      }

      model.EditActivityDialog(state, activity) -> {
        manage_projects.edit_activity_form(state, activity)
        |> input.form_dialog("Edit " <> activity.name)
      }

      model.ArchiveActivityDialog(activity) ->
        manage_projects.archive_activity_form(activity)

      model.DeleteActivityDialog(activity) ->
        manage_projects.delete_activity_form(activity)

      model.DeleteProjectDialog(project) ->
        manage_projects.delete_project_form(project)

      model.ArchiveProjectDialog(project) ->
        manage_projects.archive_project_form(project)
    }
  }

  list.map(model.dialogs, fn(dialog) { markup(dialog) })
}

fn spacer() {
  div([att.class("spacer")], [])
}
