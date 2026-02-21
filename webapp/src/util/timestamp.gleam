import gleam/int
import gleam/string
import gleam/time/calendar
import gleam/time/duration
import gleam/time/timestamp

pub fn timestamp_zero() {
  timestamp.from_unix_seconds(0)
}

/// Parse a date only ISO string to a timestamp using zero time.
///
/// # Examples
///
/// ```gleam
/// let assert Ok(ts) = timestamp.from_iso_date("2026-02-14")
/// ```
pub fn from_iso_date(date: String) -> Result(timestamp.Timestamp, Nil) {
  let rfc3339 = date <> "T00:00:00Z"
  timestamp.parse_rfc3339(rfc3339)
}

/// Convert a timestamp to an ISO date string (YYYY-MM-DD).
pub fn to_iso_date(ts: timestamp.Timestamp) -> String {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)

  let year =
    int.to_string(date.year)
    |> string.pad_start(4, "0")

  let month =
    calendar_month_to_int(date.month)
    |> int.to_string
    |> string.pad_start(2, "0")

  let day =
    int.to_string(date.day)
    |> string.pad_start(2, "0")

  year <> "-" <> month <> "-" <> day
}

/// Return a Timestamp with the same date but with the time set to zero.
pub fn with_zero_time(ts: timestamp.Timestamp) -> timestamp.Timestamp {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  timestamp.from_calendar(
    date: date,
    time: calendar.TimeOfDay(0, 0, 0, 0),
    offset: calendar.utc_offset,
  )
}

pub fn calendar_month_to_int(month: calendar.Month) -> Int {
  case month {
    calendar.January -> 1
    calendar.February -> 2
    calendar.March -> 3
    calendar.April -> 4
    calendar.May -> 5
    calendar.June -> 6
    calendar.July -> 7
    calendar.August -> 8
    calendar.September -> 9
    calendar.October -> 10
    calendar.November -> 11
    calendar.December -> 12
  }
}

/// Get a timestamp for the first day of the next month.
pub fn next_first_of_month(ts: timestamp.Timestamp) -> timestamp.Timestamp {
  let #(date, time) = timestamp.to_calendar(ts, calendar.utc_offset)

  let #(new_year, new_month) = case date.month {
    calendar.January -> #(date.year, calendar.February)
    calendar.February -> #(date.year, calendar.March)
    calendar.March -> #(date.year, calendar.April)
    calendar.April -> #(date.year, calendar.May)
    calendar.May -> #(date.year, calendar.June)
    calendar.June -> #(date.year, calendar.July)
    calendar.July -> #(date.year, calendar.August)
    calendar.August -> #(date.year, calendar.September)
    calendar.September -> #(date.year, calendar.October)
    calendar.October -> #(date.year, calendar.November)
    calendar.November -> #(date.year, calendar.December)
    calendar.December -> #(date.year + 1, calendar.January)
  }

  timestamp.from_calendar(
    date: calendar.Date(new_year, new_month, 1),
    time: time,
    offset: calendar.utc_offset,
  )
}

/// Get a timestamp for the first day of the previous month.
pub fn previous_first_of_month(ts: timestamp.Timestamp) -> timestamp.Timestamp {
  let #(date, time) = timestamp.to_calendar(ts, calendar.utc_offset)

  let #(new_year, new_month) = case date.month {
    calendar.January -> #(date.year - 1, calendar.December)
    calendar.February -> #(date.year, calendar.January)
    calendar.March -> #(date.year, calendar.February)
    calendar.April -> #(date.year, calendar.March)
    calendar.May -> #(date.year, calendar.April)
    calendar.June -> #(date.year, calendar.May)
    calendar.July -> #(date.year, calendar.June)
    calendar.August -> #(date.year, calendar.July)
    calendar.September -> #(date.year, calendar.August)
    calendar.October -> #(date.year, calendar.September)
    calendar.November -> #(date.year, calendar.October)
    calendar.December -> #(date.year, calendar.November)
  }

  timestamp.from_calendar(
    date: calendar.Date(new_year, new_month, 1),
    time: time,
    offset: calendar.utc_offset,
  )
}

/// Get a timestamp for the next day (24 hours later).
pub fn tomorrow(ts: timestamp.Timestamp) -> timestamp.Timestamp {
  timestamp.add(ts, duration.seconds(86_400))
}
