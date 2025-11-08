import gleam/time/timestamp

pub type Email {
  Email(value: String)
}

pub fn timestamp_zero() {
  timestamp.from_unix_seconds(0)
}
