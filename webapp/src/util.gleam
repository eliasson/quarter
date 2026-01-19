import gleam/time/timestamp

pub type Either(a, b) {
  Left(a)
  Right(b)
}

pub fn timestamp_zero() {
  timestamp.from_unix_seconds(0)
}
