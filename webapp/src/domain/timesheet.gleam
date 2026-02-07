import domain/duration
import gleam/time/timestamp.{type Timestamp}

pub type TimeSlot {
  TimeSlot
}

pub type Timesheet {
  Timesheet(date: Timestamp, duration: duration.Duration, slots: List(TimeSlot))
}
