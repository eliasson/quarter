import gleam/int
import gleam/string
import gleam/time/calendar
import gleam/time/timestamp

pub fn timestamp_zero() {
  timestamp.from_unix_seconds(0)
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
