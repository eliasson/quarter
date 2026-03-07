import domain/duration
import domain/project
import gleam/time/timestamp.{type Timestamp}

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
