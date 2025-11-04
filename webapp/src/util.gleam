import gleam/time/timestamp

pub fn timestamp_zero() {
  timestamp.from_unix_seconds(0)
}
