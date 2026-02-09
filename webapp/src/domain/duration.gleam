pub type Duration {
  Minutes(value: Int)
}

/// Convert a Duration to hours and minutes
/// Returns a tuple of (hours, remaining_minutes)
pub fn to_hours_and_minutes(duration: Duration) -> #(Int, Int) {
  case duration {
    Minutes(total_minutes) -> {
      let hours = total_minutes / 60
      let minutes = total_minutes % 60
      #(hours, minutes)
    }
  }
}
