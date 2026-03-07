import domain/duration
import domain/project
import gleam/dict
import gleam/list
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
  ActivityDetail(name: String, duration: duration.Duration)
}

pub type TimesheetSummary {
  TimesheetSummary(total: duration.Duration, details: List(ProjectDetail))
}

/// Get the top three activities order by their duration (descending).
pub fn top_three_activities(
  timesheet: Timesheet,
  projects: List(project.Project),
) -> List(ActivityDetail) {
  let project_lookup = dict.from_list(list.map(projects, fn(p) { #(p.id, p) }))

  let activity_name_or_default = fn(project_id, activity_id) {
    case dict.get(project_lookup, project_id) {
      Ok(p) ->
        case list.find(p.activities, fn(a) { a.id == activity_id }) {
          Ok(a) -> a.name
          Error(_) -> ""
        }
      Error(_) -> ""
    }
  }

  let result =
    timesheet.slots
    |> seq.group_by(fn(slot) { slot.activity_id })
    |> dict.map_values(fn(_, slots) {
      // Since these slots are for the same activity, just get the first one.
      let activity_name = case list.first(slots) {
        Ok(slot) -> activity_name_or_default(slot.project_id, slot.activity_id)
        _ -> panic
      }

      let quarters = list.fold(slots, 0, fn(acc, slot) { acc + slot.count })
      ActivityDetail(activity_name, duration.Minutes(quarters * 15))
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
  projects: List(project.Project),
) -> TimesheetSummary {
  // Lookup structure for project by ID
  let project_lookup = dict.from_list(list.map(projects, fn(p) { #(p.id, p) }))

  // TODO Add a ProjectList type that have these functions?
  let project_name_or_default = fn(id) {
    case dict.get(project_lookup, id) {
      Ok(p) -> p.name
      Error(_) -> ""
    }
  }

  let activity_name_or_default = fn(project_id, activity_id) {
    case dict.get(project_lookup, project_id) {
      Ok(p) ->
        case list.find(p.activities, fn(a) { a.id == activity_id }) {
          Ok(a) -> a.name
          Error(_) -> ""
        }
      Error(_) -> ""
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
          let activity_name = activity_name_or_default(project_id, activity_id)
          ActivityDetail(activity_name, duration.Minutes(total_quarters * 15))
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

fn compare_activity_detail(a: ActivityDetail, b: ActivityDetail) -> order.Order {
  duration.compare(a.duration, b.duration)
}
