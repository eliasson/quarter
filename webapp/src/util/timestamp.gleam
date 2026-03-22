import gleam/int
import gleam/list
import gleam/string
import gleam/time/calendar
import gleam/time/duration
import gleam/time/timestamp
import util/weekday

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

/// Get a timestamp for the previous day (24 hours earlier).
pub fn yesterday(ts: timestamp.Timestamp) -> timestamp.Timestamp {
  timestamp.add(ts, duration.seconds(86_400 * -1))
}

/// Check if the two timestamps represents the same date (regardless of time).
pub fn is_same_date(a: timestamp.Timestamp, b: timestamp.Timestamp) -> Bool {
  let #(date_a, _) = timestamp.to_calendar(a, calendar.utc_offset)
  let #(date_b, _) = timestamp.to_calendar(b, calendar.utc_offset)

  date_a == date_b
}

/// Returns the ISO 8601 week number (1–53) for the given timestamp.
///
/// Week 1 is the week containing the first Thursday of the year.
/// Weeks start on Monday.
pub fn iso_week_number(ts: timestamp.Timestamp) -> Int {
  let #(date, _) = timestamp.to_calendar(ts, calendar.utc_offset)
  let dow = weekday_iso_num(weekday.from_timestamp(ts))
  let doy = day_of_year(date)
  let week = { doy - dow + 10 } / 7

  case week {
    0 -> weeks_in_year(date.year - 1)
    w if w >= 53 ->
      case weeks_in_year(date.year) {
        53 -> 53
        _ -> 1
      }
    w -> w
  }
}

fn weekday_iso_num(day: weekday.Weekday) -> Int {
  case day {
    weekday.Monday -> 1
    weekday.Tuesday -> 2
    weekday.Wednesday -> 3
    weekday.Thursday -> 4
    weekday.Friday -> 5
    weekday.Saturday -> 6
    weekday.Sunday -> 7
  }
}

fn is_leap_year(year: Int) -> Bool {
  { year % 4 == 0 && year % 100 != 0 } || year % 400 == 0
}

fn days_in_month(month: calendar.Month, year: Int) -> Int {
  case month {
    calendar.January
    | calendar.March
    | calendar.May
    | calendar.July
    | calendar.August
    | calendar.October
    | calendar.December -> 31
    calendar.April | calendar.June | calendar.September | calendar.November ->
      30
    calendar.February ->
      case is_leap_year(year) {
        True -> 29
        False -> 28
      }
  }
}

fn day_of_year(date: calendar.Date) -> Int {
  let months = [
    calendar.January,
    calendar.February,
    calendar.March,
    calendar.April,
    calendar.May,
    calendar.June,
    calendar.July,
    calendar.August,
    calendar.September,
    calendar.October,
    calendar.November,
    calendar.December,
  ]
  let days_before =
    list.take(months, calendar_month_to_int(date.month) - 1)
    |> list.fold(0, fn(acc, m) { acc + days_in_month(m, date.year) })
  days_before + date.day
}

/// A year has 53 ISO weeks if Jan 1 is Thursday,
/// or if it is a leap year and Jan 1 is Wednesday.
fn weeks_in_year(year: Int) -> Int {
  let jan1 =
    timestamp.from_calendar(
      date: calendar.Date(year, calendar.January, 1),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )
  let dow = weekday_iso_num(weekday.from_timestamp(jan1))
  case dow == 4 || { dow == 3 && is_leap_year(year) } {
    True -> 53
    False -> 52
  }
}
