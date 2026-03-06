import gleam/io
import gleam/uri.{type Uri}
import lustre
import lustre/effect.{type Effect}
import message.{
  type Msg, AddUserResult, ArchiveActivity, ArchiveActivityResult,
  ArchiveProject, ArchiveProjectResult, CloseModal, ConfirmArchiveActivity,
  ConfirmArchiveProject, ConfirmDeleteActivity, ConfirmDeleteProject,
  ConfirmDialog, CreateActivityResult, CreateProjectResult, CurrentUserResult,
  DeleteActivity, DeleteActivityResult, DeleteProject, DeleteProjectResult,
  DismissError, FormTextFieldUpdated, NextMonth, NextTimesheet, Noop,
  OnRouteChange, OpenDialog, OpenDropDownMenu, PreviousMonth, PreviousTimesheet,
  ProjectsResult, SelectActivity, SystemUsersResult, TimesheetResult,
  TimesheetsResult, ToggleProject, UpdateActivityResult, UpdateProjectResult,
}
import model.{
  type Model, close_all_modals, close_modal, delete_activity, delete_project,
  dismiss_error, go_to_next_month, go_to_previous_month, go_to_tomorrow,
  go_to_yesterday, initial_model, navigate_to, open_dialog, open_drop_down_menu,
  set_current_user, set_timesheets, set_users, toggle_project, update_activity,
  update_dialog_value, update_project,
}
import modem
import protocol
import route
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
      // Called from the timesheet view, change date and load that timesheet.
      // The timesheet might already be loaded as part of the calendar, but reload it anyways to
      // always get an up-to-date version
      let m = go_to_tomorrow(model)
      let e = protocol.get_timesheet(m.today, message.TimesheetResult)
      #(m, e)
    }

    PreviousTimesheet -> {
      // Called from the timesheet view, change date and load that timesheet.
      // The timesheet might already be loaded as part of the calendar, but reload it anyways to
      // always get an up-to-date version
      let m = go_to_yesterday(model)
      let e = protocol.get_timesheet(m.today, message.TimesheetResult)
      #(m, e)
    }

    SelectActivity(id) -> {
      // Called from the timesheet view to select the activity used to "paint" the timesheet with.
      model.Model(..model, selected_activity: id)
      |> no_effect()
    }

    OpenDropDownMenu(id) -> #(open_drop_down_menu(model, id), effect.none())

    OpenDialog(d) -> #(open_dialog(model, d), effect.none())

    CloseModal -> #(close_modal(model), effect.none())

    ConfirmDialog -> handle_dialog_confirm(model)

    ToggleProject(id) -> #(toggle_project(model, id), effect.none())

    CurrentUserResult(Ok(u)) -> #(set_current_user(model, u), effect.none())

    CurrentUserResult(Error(_)) -> {
      io.println("CurrentUserResult Error")
      #(model, effect.none())
    }

    SystemUsersResult(Ok(users)) -> #(set_users(model, users), effect.none())

    SystemUsersResult(Error(_)) -> {
      io.println("SystemUsersResult Error")
      #(model, effect.none())
    }

    AddUserResult(Ok(_)) -> {
      // When the user was added successfully add the new user to the state so it
      // can be rendered in the list of users. The only way to add a new user is
      // when viewing the list of users.
      #(model, protocol.get_system_users(message.SystemUsersResult))
    }

    AddUserResult(Error(_)) -> {
      io.println("AddUserResult Error")
      #(model, effect.none())
    }

    ProjectsResult(Ok(projects)) -> #(
      model.Model(..model, projects:),
      effect.none(),
    )

    ProjectsResult(Error(_)) -> {
      io.println("ProjectsResult Error")
      #(model, effect.none())
    }

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

    ArchiveActivityResult(Error(_)) -> {
      io.println("ArchiveActivityResult Error")
      #(model, effect.none())
    }

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

    ArchiveProjectResult(Error(_)) -> {
      io.println("ArchiveProjectResult Error")
      #(model, effect.none())
    }

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
      |> delete_activity(activity.project_id, activity.id)
      |> no_effect()

    DeleteActivityResult(Error(_)) -> #(model, effect.none())

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

    DeleteProjectResult(Error(_)) -> {
      io.println("DeleteProjectResult Error")

      #(model, effect.none())
    }

    CreateProjectResult(Ok(_)) -> #(
      model,
      protocol.get_projects_and_activities(message.ProjectsResult),
    )

    CreateProjectResult(Error(_)) -> {
      io.println("CreateProjectResult Error")
      #(model, effect.none())
    }

    UpdateProjectResult(Ok(p)) ->
      model
      |> close_all_modals()
      |> update_project(p)
      |> no_effect()

    UpdateProjectResult(Error(_)) -> {
      io.println("UpdateProjectResult Error")
      #(model, effect.none())
    }

    CreateActivityResult(Ok(_)) -> #(
      model,
      protocol.get_projects_and_activities(message.ProjectsResult),
    )

    CreateActivityResult(Error(_)) -> {
      io.println("CreateActivityResult Error")
      #(model, effect.none())
    }

    UpdateActivityResult(Ok(a)) ->
      model
      |> close_all_modals()
      |> update_activity(a)
      |> no_effect()

    UpdateActivityResult(Error(_)) -> {
      io.println("UpdateActivityResult Error")
      #(model, effect.none())
    }

    TimesheetsResult(Ok(timesheets)) -> {
      model
      |> set_timesheets(timesheets)
      |> no_effect()
    }

    TimesheetsResult(Error(_)) -> {
      io.println("TimesheetsResult Error")
      #(model, effect.none())
    }

    TimesheetResult(Ok(_timesheet)) -> {
      io.println("TimesheetResult Ok")
      #(model, effect.none())
    }

    TimesheetResult(Error(_)) -> {
      io.println("TimesheetResult Error")
      #(model, effect.none())
    }
  }
}

fn on_url_change(uri: Uri) -> Msg {
  route.identify(uri) |> OnRouteChange()
}

/// Get the efffect (if any) to apply when loading a route.
fn effect_on_route_loaded(m: model.Model, r: route.Route) {
  case r {
    route.Home -> protocol.get_timesheets(m.today, message.TimesheetsResult)
    // TODO load timesheet on timesheet view
    route.AdministerSystemUsers ->
      protocol.get_system_users(message.SystemUsersResult)

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
