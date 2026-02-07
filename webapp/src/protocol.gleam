import domain/color
import domain/duration
import domain/email
import domain/project
import domain/timesheet
import domain/user
import gleam/dict
import gleam/dynamic/decode
import gleam/http/response.{type Response}
import gleam/int
import gleam/json
import gleam/list
import gleam/option.{type Option, None, Some}
import gleam/time/timestamp
import lustre/effect.{type Effect}
import message
import rsvp
import seq
import util/timestamp as tsutil

/// Get the currently logged in user, if logged in.
pub fn get_current_user(
  on_response handle_response: fn(Result(user.User, rsvp.Error)) -> message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/users/self"
  let handler = rsvp.expect_json(user_resource_decoder(), handle_response)

  rsvp.get(url, handler)
}

/// Get all the system users.
/// This HTTP call requires the current user to have sufficient access to do
/// so, else the request will fail with Forbidden.
pub fn get_system_users(
  on_response handle_response: fn(Result(List(user.User), rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/users"

  let handler =
    rsvp.expect_json(decode.list(user_resource_decoder()), handle_response)

  rsvp.get(url, handler)
}

pub fn add_user(
  email: email.Email,
  on_response handle_response: fn(Result(user.User, rsvp.Error)) -> message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/users"
  let handler = rsvp.expect_json(user_resource_decoder(), handle_response)
  let payload = json.object([#("email", json.string(email.value))])

  rsvp.post(url, payload, handler)
}

pub fn create_project(
  name: String,
  description: String,
  on_response handle_response: fn(Result(project.Project, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/projects"
  let handler = rsvp.expect_json(project_decoder(), handle_response)
  let payload =
    json.object([
      #("name", json.string(name)),
      #("description", json.string(description)),
    ])

  rsvp.post(url, payload, handler)
}

pub fn update_project(
  project: project.Project,
  name: String,
  description: String,
  on_response handle_response: fn(Result(project.Project, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let handler = rsvp.expect_json(project_decoder(), handle_response)
  let payload =
    json.object([
      #("name", json.string(name)),
      #("description", json.string(description)),
    ])

  rsvp.patch(project_url(project), payload, handler)
}

pub fn create_activity(
  project: project.Project,
  name: String,
  description: String,
  color_value: color.Color,
  on_response handle_response: fn(Result(project.Activity, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/projects/" <> project.id.value <> "/activities"
  let handler = rsvp.expect_json(activity_decoder(), handle_response)
  let payload =
    json.object([
      #("name", json.string(name)),
      #("description", json.string(description)),
      #("color", json.string(color.to_hex(color_value))),
    ])

  rsvp.post(url, payload, handler)
}

pub fn update_activity(
  activity: project.Activity,
  name: String,
  description: String,
  color_value: color.Color,
  on_response handle_response: fn(Result(project.Activity, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let handler = rsvp.expect_json(activity_decoder(), handle_response)
  let payload =
    json.object([
      #("name", json.string(name)),
      #("description", json.string(description)),
      #("color", json.string(color.to_hex(color_value))),
    ])

  rsvp.patch(activity_url(activity), payload, handler)
}

/// Get all users projects and activities in a single request.
pub fn get_projects_and_activities(
  on_response handle_response: fn(Result(List(project.Project), rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let url = "/api/projects/activities"

  let handler =
    rsvp.expect_json(project_and_activities_decoder(), handle_response)

  rsvp.get(url, handler)
}

pub fn archive_activity(
  activity: project.Activity,
  on_response handle_response: fn(Result(project.Activity, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let handler = rsvp.expect_json(activity_decoder(), handle_response)
  let payload = json.object([#("isArchived", json.bool(!activity.is_archived))])

  rsvp.patch(activity_url(activity), payload, handler)
}

pub fn delete_activity(
  activity: project.Activity,
  on_response handle_response: fn(Result(project.Activity, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let handler = fn(result: Result(Response(String), rsvp.Error)) -> message.Msg {
    case result {
      Ok(_) -> handle_response(Ok(activity))
      Error(e) -> handle_response(Error(e))
    }
  }

  rsvp.delete(
    activity_url(activity),
    json.object([]),
    rsvp.expect_ok_response(handler),
  )
}

pub fn archive_project(
  project: project.Project,
  on_response handle_response: fn(Result(project.Project, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let handler = rsvp.expect_json(project_decoder(), handle_response)
  let payload = json.object([#("isArchived", json.bool(!project.is_archived))])

  rsvp.patch(project_url(project), payload, handler)
}

pub fn delete_project(
  project: project.Project,
  on_response handle_response: fn(Result(project.Project, rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let handler = fn(result: Result(Response(String), rsvp.Error)) -> message.Msg {
    case result {
      Ok(_) -> handle_response(Ok(project))
      Error(e) -> handle_response(Error(e))
    }
  }

  rsvp.delete(
    project_url(project),
    json.object([]),
    rsvp.expect_ok_response(handler),
  )
}

/// Get all timesheets for a given year and month.
pub fn get_timesheets(
  year: Int,
  month: Int,
  on_response handle_response: fn(Result(List(timesheet.Timesheet), rsvp.Error)) ->
    message.Msg,
) -> Effect(message.Msg) {
  let url =
    "/api/timesheets/" <> int.to_string(year) <> "/" <> int.to_string(month)

  let handler = rsvp.expect_json(timesheets_response_decoder(), handle_response)

  rsvp.get(url, handler)
}

// Decoders ------------------------------------------------------------------

pub fn user_resource_decoder() -> decode.Decoder(user.User) {
  use id <- decode.field("id", decode.string)
  use email <- decode.field("email", decode.string)
  use created <- decode.field("created", decode_timestamp())
  use updated <- decode.optional_field(
    "updated",
    option.None,
    decode_optional_timestamp(),
  )

  decode.success(user.User(id:, email:, created:, updated:))
}

pub fn project_and_activities_decoder() -> decode.Decoder(List(project.Project)) {
  use projects_without_activities <- decode.field(
    "projects",
    decode.list(project_decoder()),
  )
  use all_activities <- decode.field(
    "activities",
    decode.list(activity_decoder()),
  )

  let lookup =
    list.fold(all_activities, dict.new(), fn(acc, a) {
      seq.add_or_append(acc, a.project_id, a)
    })

  let projects =
    list.map(projects_without_activities, fn(p) {
      project.Project(..p, activities: seq.get_or_empty(lookup, p.id))
    })

  decode.success(projects)
}

pub fn project_decoder() -> decode.Decoder(project.Project) {
  use id <- decode.field("id", decode.string)
  use name <- decode.field("name", decode.string)
  use description <- decode.field("description", decode.string)
  use is_archived <- decode.field("isArchived", decode.bool)
  use created <- decode.field("created", decode_timestamp())
  use updated <- decode.optional_field(
    "updated",
    option.None,
    decode_optional_timestamp(),
  )

  decode.success(
    project.Project(
      project.ProjectId(id),
      name,
      description,
      is_archived,
      created,
      updated,
      activities: [],
    ),
  )
}

pub fn activity_decoder() -> decode.Decoder(project.Activity) {
  use id <- decode.field("id", decode.string)
  use project_id <- decode.field("projectId", decode.string)
  use name <- decode.field("name", decode.string)
  use description <- decode.field("description", decode.string)
  use color_field <- decode.field("color", decode_color())
  use is_archived <- decode.field("isArchived", decode.bool)
  use created <- decode.field("created", decode_timestamp())
  use updated <- decode.optional_field(
    "updated",
    option.None,
    decode_optional_timestamp(),
  )

  decode.success(project.Activity(
    project.ActivityId(id),
    project.ProjectId(project_id),
    name,
    description,
    color_field,
    is_archived,
    created,
    updated,
  ))
}

/// Decode a ISO-8601 / RFC-3339 timestamp from a string.
fn decode_timestamp() -> decode.Decoder(timestamp.Timestamp) {
  use ts_str <- decode.then(decode.string)

  case timestamp.parse_rfc3339(ts_str) {
    Ok(ts) -> decode.success(ts)
    _ -> decode.failure(tsutil.timestamp_zero(), "Could not parse timestamp")
  }
}

/// Decode an optional ISO-8601 / RFC-3339 timestamp from a string.
fn decode_optional_timestamp() -> decode.Decoder(Option(timestamp.Timestamp)) {
  use ts_str <- decode.then(decode.optional(decode.string))

  case ts_str {
    Some(ts) ->
      case timestamp.parse_rfc3339(ts) {
        Ok(t) -> decode.success(Some(t))
        _ -> decode.failure(None, "Could not parse timestamp")
      }

    None -> decode.success(None)
  }
}

fn decode_color() -> decode.Decoder(color.Color) {
  use color_hex <- decode.then(decode.string)

  case color.from_hex(color_hex) {
    Ok(c) -> decode.success(c)
    _ -> decode.failure(color.Color(0, 0, 0), "Could not parse color")
  }
}

pub fn timesheets_response_decoder() -> decode.Decoder(
  List(timesheet.Timesheet),
) {
  use timesheets <- decode.field("timesheets", decode.list(timesheet_decoder()))
  decode.success(timesheets)
}

pub fn timesheet_decoder() -> decode.Decoder(timesheet.Timesheet) {
  use date <- decode.field("date", decode_timestamp())
  use total_minutes <- decode.field("totalMinutes", decode.int)

  decode.success(
    timesheet.Timesheet(
      date:,
      duration: duration.Minutes(total_minutes),
      slots: [],
    ),
  )
}

/// Construct the absolut path of the URL used to perform activity related operations.
pub fn activity_url(activity: project.Activity) -> String {
  "/api/projects/"
  <> activity.project_id.value
  <> "/activities/"
  <> activity.id.value
}

pub fn project_url(project: project.Project) -> String {
  "/api/projects/" <> project.id.value
}
