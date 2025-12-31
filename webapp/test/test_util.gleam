import gleam/option
import project
import util

/// Get the first element from a tuple
pub fn first(t: #(a, b)) {
  t.0
}

pub fn arbitrary_activity() -> project.Activity {
  project.Activity(
    project.ActivityId("A01"),
    project.ProjectId("P01"),
    "Activity Alpha",
    "",
    util.Color(142, 135, 245),
    False,
    util.timestamp_zero(),
    option.None,
  )
}
