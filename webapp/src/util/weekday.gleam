import gleam/time/timestamp.{type Timestamp}

/// Days of the week
pub type Weekday {
  Sunday
  Monday
  Tuesday
  Wednesday
  Thursday
  Friday
  Saturday
}

/// Convert a Timestamp to a Weekday using JavaScript's Date API
pub fn from_timestamp(ts: Timestamp) -> Weekday {
  let #(seconds, nanoseconds) = timestamp.to_unix_seconds_and_nanoseconds(ts)
  let millis = seconds * 1000 + nanoseconds / 1_000_000
  let day_num = get_day_of_week_ffi(millis)
  from_int(day_num)
}

@external(javascript, "../ffi/date_ffi.mjs", "getDayOfWeek")
fn get_day_of_week_ffi(unix_millis: Int) -> Int

fn from_int(day: Int) -> Weekday {
  case day {
    0 -> Sunday
    1 -> Monday
    2 -> Tuesday
    3 -> Wednesday
    4 -> Thursday
    5 -> Friday
    _ -> Saturday
  }
}
