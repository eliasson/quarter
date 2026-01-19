import domain/color
import gleam/option
import gleam/time/timestamp.{type Timestamp}

pub type ProjectId {
  ProjectId(value: String)
}

pub type ActivityId {
  ActivityId(value: String)
}

pub type Project {
  Project(
    id: ProjectId,
    name: String,
    description: String,
    is_archived: Bool,
    created: Timestamp,
    updated: option.Option(Timestamp),
    activities: List(Activity),
  )
}

pub type Activity {
  Activity(
    id: ActivityId,
    project_id: ProjectId,
    name: String,
    description: String,
    color: color.Color,
    is_archived: Bool,
    created: Timestamp,
    updated: option.Option(Timestamp),
  )
}
