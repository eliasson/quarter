import domain/color
import domain/duration
import domain/project
import gleam/dict
import gleam/int
import gleam/list
import gleam/option
import gleam/order
import gleam/time/timestamp.{type Timestamp}
import util/seq

pub type TimeSlot {

  /// A time slot represents a single continous time registration for an activity.
  /// project_id: The ID of the project.
  /// activity_id: The ID of the activity.
  /// offset: The zero based quarter index (0-95).
  /// count: The number of quarters used (i.e. the duration of the slot).
  TimeSlot(
    project_id: project.ProjectId,
    activity_id: project.ActivityId,
    offset: Int,
    count: Int,
  )
}

pub type Timesheet {
  Timesheet(date: Timestamp, duration: duration.Duration, slots: List(TimeSlot))
}

pub type ProjectDetail {
  ProjectDetail(
    name: String,
    duration: duration.Duration,
    activities: List(ActivityDetail),
  )
}

pub type ActivityDetail {
  ActivityDetail(
    name: String,
    duration: duration.Duration,
    color: color.Color,
    border_color: color.Color,
  )
}

pub type TimesheetSummary {
  TimesheetSummary(total: duration.Duration, details: List(ProjectDetail))
}

pub type QuarterDetail {
  /// The details for a quarter of an hour. The activity and the offset it represents.
  QuarterDetail(activity: project.Activity, offset: Int)
}

/// Represents a single hour in a timesheet, with four quarters each tied to an optional detail.
pub type TimesheetHour {
  TimesheetHour(
    hour: Int,
    q1: option.Option(QuarterDetail),
    q2: option.Option(QuarterDetail),
    q3: option.Option(QuarterDetail),
    q4: option.Option(QuarterDetail),
  )
}

/// Get the top three activities order by their duration (descending).
pub fn top_three_activities(
  timesheet: Timesheet,
  projects: project.ProjectCollection,
) -> List(ActivityDetail) {
  let activity_or_default = fn(_project_id, activity_id) {
    case project.get_activity(projects, activity_id) {
      Ok(a) -> #(a.name, a.color)
      Error(_) -> #("", color.Color(0, 0, 0))
    }
  }

  let result =
    timesheet.slots
    |> seq.group_by(fn(slot) { slot.activity_id })
    |> dict.map_values(fn(_, slots) {
      // Since these slots are for the same activity, just get the first one.
      let #(activity_name, activity_color) = case list.first(slots) {
        Ok(slot) -> activity_or_default(slot.project_id, slot.activity_id)
        _ -> panic
      }

      let quarters = list.fold(slots, 0, fn(acc, slot) { acc + slot.count })
      ActivityDetail(
        activity_name,
        duration.Minutes(quarters * 15),
        activity_color,
        color.darken(activity_color),
      )
    })
    |> dict.to_list
    |> list.map(fn(pair) {
      let #(_, detail) = pair
      detail
    })
    |> list.sort(by: compare_activity_detail)
    |> list.reverse
    |> list.take(3)

  result
}

pub fn summary(
  timesheet: Timesheet,
  projects: project.ProjectCollection,
) -> TimesheetSummary {
  let project_name_or_default = fn(id) {
    case project.get_project(projects, id) {
      Ok(p) -> p.name
      Error(_) -> ""
    }
  }

  let activity_or_default = fn(_project_id, activity_id) {
    case project.get_activity(projects, activity_id) {
      Ok(a) -> #(a.name, a.color)
      Error(_) -> #("", color.Color(0, 0, 0))
    }
  }

  let slots_by_project =
    seq.group_by(timesheet.slots, fn(slot) { slot.project_id })

  let details =
    dict.to_list(slots_by_project)
    |> list.map(fn(pair) {
      let #(project_id, slots) = pair

      let slots_by_activity = seq.group_by(slots, fn(slot) { slot.activity_id })

      let activity_details =
        dict.to_list(slots_by_activity)
        |> list.map(fn(activity_pair) {
          let #(activity_id, activity_slots) = activity_pair
          let total_quarters =
            list.fold(activity_slots, 0, fn(acc, slot) { acc + slot.count })
          let #(activity_name, activity_color) =
            activity_or_default(project_id, activity_id)
          ActivityDetail(
            activity_name,
            duration.Minutes(total_quarters * 15),
            activity_color,
            color.darken(activity_color),
          )
        })

      let project_name = project_name_or_default(project_id)

      let project_duration =
        activity_details
        |> list.fold(0, fn(acc, ad) { acc + ad.duration.value })
        |> duration.Minutes()

      ProjectDetail(project_name, project_duration, activity_details)
    })

  let total_quarters =
    list.fold(timesheet.slots, 0, fn(acc, slot) { acc + slot.count })

  TimesheetSummary(duration.Minutes(total_quarters * 15), details)
}

/// Get the hours for a given timesheet between the specified start and end hours (inclusive).
/// Each TimesheetHour has four quarters, each optionally occupied by an ActivityDetail.
pub fn hours(
  timesheet: Timesheet,
  start_hour: Int,
  end_hour: Int,
  projects: project.ProjectCollection,
) -> List(TimesheetHour) {
  let quarter_lookup =
    list.fold(timesheet.slots, dict.new(), fn(acc, slot) {
      // Create the base quarter details that represents this slot.
      // This needs to be modified to set the correct offset based on the actual quarter.
      let maybe_details = case
        project.get_activity(projects, slot.activity_id)
      {
        Ok(a) -> option.Some(QuarterDetail(a, slot.offset))
        Error(_) -> option.None
      }

      case maybe_details {
        option.None -> acc
        option.Some(d) ->
          // For the count / duration of this slot, insert this quarter detail keyed by
          // the quarter offset (0-96)
          int.range(slot.offset, slot.offset + slot.count, acc, fn(acc, q) {
            // Each offset needs a unqiue quarter detail as these carry the offset they represent.
            // Which make the time registration a lot simpler
            let quarter = QuarterDetail(..d, offset: q)
            dict.insert(acc, q, quarter)
          })
      }
    })

  let quarter_detail = fn(offset) {
    option.from_result(dict.get(quarter_lookup, offset))
  }

  int.range(start_hour, end_hour + 1, [], fn(acc, hour) {
    let base = hour * 4
    list.append(acc, [
      TimesheetHour(
        hour: hour,
        q1: quarter_detail(base),
        q2: quarter_detail(base + 1),
        q3: quarter_detail(base + 2),
        q4: quarter_detail(base + 3),
      ),
    ])
  })
}

fn compare_activity_detail(a: ActivityDetail, b: ActivityDetail) -> order.Order {
  duration.compare(a.duration, b.duration)
}
