import domain/duration
import domain/project
import gleam/time/timestamp.{type Timestamp}

pub type ActivityUsage {
  ActivityUsage(
    activity_id: project.ActivityId,
    duration: duration.Duration,
    weekday_totals: List(duration.Duration),
  )
}

pub type ProjectUsage {
  ProjectUsage(
    project_id: project.ProjectId,
    duration: duration.Duration,
    activity_usage: List(ActivityUsage),
  )
}

pub type WeeklyReport {
  WeeklyReport(
    start_of_week: Timestamp,
    end_of_week: Timestamp,
    duration: duration.Duration,
    weekday_totals: List(duration.Duration),
    usage: List(ProjectUsage),
  )
}
