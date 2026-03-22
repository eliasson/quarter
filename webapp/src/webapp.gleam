import gleam/option
import gleam/time/calendar
import gleam/time/timestamp
import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message.{
  type Msg, AddUserResult, ArchiveActivity, ArchiveActivityResult,
  ArchiveProject, ArchiveProjectResult, CloseModal, CommitRegistering,
  ConfirmArchiveActivity, ConfirmArchiveProject, ConfirmDeleteActivity,
  ConfirmDeleteProject, ConfirmDialog, CreateActivityResult, CreateProjectResult,
  CurrentUserResult, DeleteActivity, DeleteActivityResult, DeleteProject,
  DeleteProjectResult, DismissError, ExtendEndOfDay, ExtendStartOfDay,
  FormTextFieldUpdated, Logout, NextMonth, NextReportWeek, NextTimesheet, Noop,
  OnRouteChange, OpenDialog, OpenDropDownMenu, PreviousMonth, PreviousReportWeek,
  PreviousTimesheet, ProjectsResult, RegisterTimeResult, ReportResult,
  SelectActivity, StartRegistering, SystemUsersResult, TimesheetResult,
  TimesheetsResult, ToggleActivityPicker, ToggleProject, UpdateActivityResult,
  UpdateProjectResult, UpdateRegistering,
}
import model.{
  type Model, add_error, clear_registration, close_all_modals, close_modal,
  delete_activity, delete_project, dismiss_error, extend_end_of_day,
  extend_start_of_day, go_to_next_month, go_to_previous_month, initial_model,
  navigate_to, open_dialog, open_drop_down_menu, select_activity, select_quarter,
  set_active_report, set_active_timesheet, set_current_user, set_timesheets,
  set_users, start_registration, toggle_activity_picker, toggle_project,
  update_activity, update_dialog_value, update_project,
}

import modem
import protocol
import route
import util/timestamp as tsutil
import view

pub fn main() {
  let app = lustre.application(init, update, view.view)
  let assert Ok(_) = lustre.start(app, "#quarter", Nil)

  Nil
}

fn init(_args) -> #(Model, Effect(Msg)) {
  // Get the inital URL to match the route to set in the model. This is when a deep-link or
  // page refresh occurs.
  let initial_route = case modem.initial_uri() {
    Ok(uri) -> route.identify(uri)
    _ -> route.Home
  }

  let model = initial_model() |> navigate_to(initial_route)

  // At load we need to trigger two effects. One for setting up the URL routing and one to fetch
  // the current users to see if this user is authenticated or not.
  let init_effects =
    effect.batch([
      // Setup the router to monitor for URL changes.
      modem.init(on_url_change),
      // Load the current user.
      protocol.get_current_user(message.CurrentUserResult),
      // Get all available projects and activities.
      protocol.get_projects_and_activities(message.ProjectsResult),
      // Load any additional data based on route.
      effect_on_route_loaded(model, initial_route),
    ])

  #(model, init_effects)
}

/// The orchestration of the application. All user actions, HTTP responses, etc. is dispatched
/// here as messages together with the current model.
/// Perform updates to the model and triggers optional HTTP requests, etc. as effects.
pub fn update(model: Model, msg: Msg) -> #(Model, Effect(Msg)) {
  case msg {
    Noop -> model |> close_all_modals |> no_effect()

    Logout -> {
      let assert Ok(uri) = uri.parse(route.logout_url)
      #(model, modem.load(uri))
    }

    OnRouteChange(r) -> {
      let m =
        model
        |> close_all_modals
        |> navigate_to(r)

      let eff =
        m
        |> effect_on_route_loaded(r)

      #(m, eff)
    }

    NextMonth -> {
      // Called from the home/calendar view, change month and load a new set of timesheets.
      let m = go_to_next_month(model)
      let e = protocol.get_timesheets(m.today, message.TimesheetsResult)
      #(m, e)
    }

    PreviousMonth -> {
      // Called from the home/calendar view, change month and load a new set of timesheets.
      let m = go_to_previous_month(model)
      let e = protocol.get_timesheets(m.today, message.TimesheetsResult)
      #(m, e)
    }

    NextTimesheet -> {
      // Called from the timesheet view to move to tomorrow's timesheet.
      let e =
        tsutil.tomorrow(model.today)
        |> route.Timesheet()
        |> route.to_url()
        |> modem.push(option.None, option.None)

      #(model, e)
    }

    PreviousTimesheet -> {
      // Called from the timesheet view to move to yesterday's timesheet.

      let e =
        tsutil.yesterday(model.today)
        |> route.Timesheet()
        |> route.to_url()
        |> modem.push(option.None, option.None)

      #(model, e)
    }

    SelectActivity(activity) ->
      // Called from the timesheet view to select the activity used to "paint" the timesheet with.
      select_activity(model, activity) |> no_effect()

    StartRegistering(index) ->
      model
      |> start_registration(index)
      |> no_effect()

    UpdateRegistering(index) ->
      model
      |> select_quarter(index)
      |> no_effect()

    CommitRegistering -> {
      case model.active_registration {
        option.Some(reg) ->
          model
          |> with_effect(protocol.register_time(
            model.today,
            reg,
            message.RegisterTimeResult,
          ))
        option.None -> model |> no_effect()
      }
    }

    RegisterTimeResult(Ok(timesheet)) ->
      model
      |> set_active_timesheet(timesheet)
      |> clear_registration()
      |> no_effect()

    RegisterTimeResult(Error(_)) ->
      model
      |> with_error("Unable to register time.")
      |> clear_registration()
      |> no_effect()

    ExtendStartOfDay -> extend_start_of_day(model) |> no_effect()

    ExtendEndOfDay -> extend_end_of_day(model) |> no_effect()

    ToggleActivityPicker -> toggle_activity_picker(model) |> no_effect()

    OpenDropDownMenu(id) -> #(open_drop_down_menu(model, id), effect.none())

    OpenDialog(d) -> #(open_dialog(model, d), effect.none())

    CloseModal -> #(close_modal(model), effect.none())

    ConfirmDialog -> handle_dialog_confirm(model)

    ToggleProject(id) -> #(toggle_project(model, id), effect.none())

    CurrentUserResult(Ok(u)) -> #(set_current_user(model, u), effect.none())

    CurrentUserResult(Error(_)) ->
      model
      |> with_error("Unable to get current user.")
      |> no_effect()

    SystemUsersResult(Ok(users)) -> #(set_users(model, users), effect.none())

    SystemUsersResult(Error(_)) ->
      model
      |> with_error("Unable to get users.")
      |> no_effect()

    AddUserResult(Ok(_)) -> {
      // When the user was added successfully add the new user to the state so it
      // can be rendered in the list of users. The only way to add a new user is
      // when viewing the list of users.
      #(model, protocol.get_system_users(message.SystemUsersResult))
    }

    AddUserResult(Error(_)) ->
      model
      |> with_error("Unable to add user.")
      |> no_effect()

    ProjectsResult(Ok(projects)) -> #(
      model.Model(..model, projects:),
      effect.none(),
    )

    ProjectsResult(Error(_)) ->
      model
      |> with_error("Unable to load projects.")
      |> no_effect()

    FormTextFieldUpdated(value) -> #(
      update_dialog_value(model, value),
      effect.none(),
    )

    DismissError(id) -> #(dismiss_error(model, id), effect.none())

    ConfirmArchiveActivity(activity) ->
      model
      |> open_dialog(model.ArchiveActivityDialog(activity))
      |> no_effect()

    ArchiveActivity(activity) ->
      model
      |> close_all_modals()
      |> with_effect(protocol.archive_activity(
        activity,
        message.ArchiveActivityResult,
      ))

    ArchiveActivityResult(Ok(a)) ->
      model
      |> close_all_modals()
      |> update_activity(a)
      |> no_effect()

    ArchiveActivityResult(Error(_)) ->
      model
      |> with_error("Unable to archive activity.")
      |> no_effect()

    ConfirmArchiveProject(project) ->
      model
      |> open_dialog(model.ArchiveProjectDialog(project))
      |> no_effect()

    ArchiveProject(project) ->
      model
      |> close_all_modals()
      |> with_effect(protocol.archive_project(
        project,
        message.ArchiveProjectResult,
      ))

    ArchiveProjectResult(Ok(p)) ->
      model
      |> close_all_modals()
      |> update_project(p)
      |> no_effect()

    ArchiveProjectResult(Error(_)) ->
      model
      |> with_error("Unable to archive project.")
      |> no_effect()

    ConfirmDeleteActivity(activity) ->
      model
      |> open_dialog(model.DeleteActivityDialog(activity))
      |> no_effect()

    DeleteActivity(activity) ->
      model
      |> close_all_modals()
      |> with_effect(protocol.delete_activity(
        activity,
        message.DeleteActivityResult,
      ))

    DeleteActivityResult(Ok(activity)) ->
      model
      |> close_all_modals()
      |> delete_activity(activity.id)
      |> no_effect()

    DeleteActivityResult(Error(_)) ->
      model
      |> with_error("Unable to delete activity.")
      |> no_effect()

    ConfirmDeleteProject(project) ->
      model
      |> open_dialog(model.DeleteProjectDialog(project))
      |> no_effect()

    DeleteProject(project) ->
      model
      |> close_all_modals()
      |> with_effect(protocol.delete_project(
        project,
        message.DeleteProjectResult,
      ))

    DeleteProjectResult(Ok(project)) ->
      model
      |> close_all_modals()
      |> delete_project(project.id)
      |> no_effect()

    DeleteProjectResult(Error(_)) ->
      model
      |> with_error("Unable to delete project.")
      |> no_effect()

    CreateProjectResult(Ok(_)) -> #(
      model,
      protocol.get_projects_and_activities(message.ProjectsResult),
    )

    CreateProjectResult(Error(_)) ->
      model
      |> with_error("Unable to create project.")
      |> no_effect()

    UpdateProjectResult(Ok(p)) ->
      model
      |> close_all_modals()
      |> update_project(p)
      |> no_effect()

    UpdateProjectResult(Error(_)) ->
      model
      |> with_error("Unable to update project.")
      |> no_effect()

    CreateActivityResult(Ok(_)) -> #(
      model,
      protocol.get_projects_and_activities(message.ProjectsResult),
    )

    CreateActivityResult(Error(_)) ->
      model
      |> with_error("Unable to create activity.")
      |> no_effect()

    UpdateActivityResult(Ok(a)) ->
      model
      |> close_all_modals()
      |> update_activity(a)
      |> no_effect()

    UpdateActivityResult(Error(_)) ->
      model
      |> with_error("Unable to update activity.")
      |> no_effect()

    TimesheetsResult(Ok(timesheets)) ->
      model
      |> set_timesheets(timesheets)
      |> no_effect()

    TimesheetsResult(Error(_)) ->
      model
      |> with_error("Unable to load timesheets.")
      |> no_effect()

    TimesheetResult(Ok(timesheet)) ->
      set_active_timesheet(model, timesheet)
      |> no_effect()

    TimesheetResult(Error(_)) ->
      model
      |> with_error("Unable to load timesheet.")
      |> no_effect()

    NextReportWeek -> {
      // Called from the report view to move to next week's report.
      //
      // There should always be a current report which have a end of week timestamp.
      // Use that to go one day later and get the report for that week.
      case model.active_report {
        option.Some(report) -> {
          let e =
            tsutil.tomorrow(report.end_of_week)
            |> route.Report()
            |> route.to_url()
            |> modem.push(option.None, option.None)
          #(model, e)
        }
        _ -> #(model, effect.none())
      }
    }

    PreviousReportWeek -> {
      // Called from the report view to move to previous week's report.
      //
      // There should always be a current report which have a start of week timestamp.
      // Use that to go one day earlier and get the report for that week.
      case model.active_report {
        option.Some(report) -> {
          let e =
            tsutil.yesterday(report.start_of_week)
            |> route.Report()
            |> route.to_url()
            |> modem.push(option.None, option.None)
          #(model, e)
        }
        _ -> #(model, effect.none())
      }
    }

    ReportResult(Ok(report)) ->
      set_active_report(model, report)
      |> no_effect()

    ReportResult(Error(_)) ->
      model
      |> with_error("Unable to load report.")
      |> no_effect()
  }
}

fn on_url_change(uri: Uri) -> Msg {
  route.identify(uri) |> OnRouteChange()
}

/// Get the efffect (if any) to apply when loading a route.
fn effect_on_route_loaded(m: model.Model, r: route.Route) {
  case r {
    route.Home -> protocol.get_timesheets(m.today, message.TimesheetsResult)

    route.Timesheet(date) ->
      protocol.get_timesheet(date, message.TimesheetResult)

    // TODO load timesheet on timesheet view
    route.AdministerSystemUsers ->
      protocol.get_system_users(message.SystemUsersResult)

    route.Report(date) -> protocol.get_weekly_report(date, message.ReportResult)

    _ -> effect.none()
  }
}

fn handle_dialog_confirm(m: model.Model) {
  let work = case model.current_dialog(m) {
    Ok(model.AddUserDialog(state)) ->
      protocol.add_user(state.email.value, message.AddUserResult)

    Ok(model.AddProjectDialog(state)) ->
      protocol.create_project(
        state.name.value,
        state.description.value,
        message.CreateProjectResult,
      )

    Ok(model.EditProjectDialog(state, project)) ->
      protocol.update_project(
        project,
        state.name.value,
        state.description.value,
        message.UpdateProjectResult,
      )

    Ok(model.AddActivityDialog(state, project)) ->
      protocol.create_activity(
        project,
        state.name.value,
        state.description.value,
        state.color.value,
        message.CreateActivityResult,
      )

    Ok(model.EditActivityDialog(state, activity)) ->
      protocol.update_activity(
        activity,
        state.name.value,
        state.description.value,
        state.color.value,
        message.UpdateActivityResult,
      )

    _ -> effect.none()
  }

  #(close_modal(m), work)
}

fn no_effect(m: Model) {
  #(m, effect.none())
}

fn with_effect(m: Model, e: effect.Effect(Msg)) {
  #(m, e)
}

fn with_error(m: Model, message: String) -> model.Model {
  // Generate a unique error ID so that errors can be dismissed.
  let id =
    timestamp.system_time()
    |> timestamp.to_rfc3339(calendar.utc_offset)

  add_error(m, model.ApplicationError(id:, message:))
}
