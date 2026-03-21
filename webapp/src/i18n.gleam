/// Our homebrewd i18n solution.
///
/// Keep i18n minimal and preferrably type safe. The idea is that this module will deal with
/// all translations and internationalization, not any type specific module.
///
/// Quarter uses relativly few translations (I think), so we can keep it this simple.
import domain/duration
import gleam/float
import gleam/int
import gleam/string
import gleam/time/calendar
import gleam/time/timestamp
import util/weekday

pub type Language {
  English
}

pub type Translation {
  Translation(value: String)
}

/// Describe a translation without any formatting.
pub fn describe(t: Translation) -> String {
  t.value
}

/// Describe a translation using capitalization on first letter.
pub fn capitalize(t: Translation) -> String {
  string.capitalise(t.value)
}

/// Returns the name of the month for the given timestamp.
pub fn name_of_month(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  Translation(calendar_month(date.month))
}

/// Returns the name of the day for the given timestamp.
pub fn name_of_day(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let weekday = weekday.from_timestamp(ts)
  Translation(weekday_name(weekday))
}

/// Returns the day of the month for the given timestamp.
/// Eg. "Mon 16", or "Fri 20"
pub fn day_short(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  let weekday = weekday.from_timestamp(ts)
  Translation(weekday_name_short(weekday) <> " " <> int.to_string(date.day))
}

/// Returns the year for the given timestamp.
pub fn year(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  Translation(int.to_string(date.year))
}

/// Returns the day of the month for the given timestamp.
pub fn day(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  Translation(int.to_string(date.day))
}

/// Formats the date in a long format (e.g. "Saturday, 14 february").
pub fn date_long(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)

  let weekday = weekday.from_timestamp(ts)

  // Saturday, 14 february
  Translation(
    weekday_name(weekday)
    <> ", "
    <> int.to_string(date.day)
    <> " "
    <> calendar_month(date.month),
  )
}

/// Formats the date in a short format (e.g. "14 february 2023").
pub fn date_short(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)

  // 14 february
  Translation(
    int.to_string(date.day)
    <> " "
    <> calendar_month(date.month)
    <> " "
    <> int.to_string(date.year),
  )
}

fn calendar_month(month: calendar.Month) -> String {
  case month {
    calendar.January -> "january"
    calendar.February -> "february"
    calendar.March -> "march"
    calendar.April -> "april"
    calendar.May -> "may"
    calendar.June -> "june"
    calendar.July -> "july"
    calendar.August -> "august"
    calendar.September -> "september"
    calendar.October -> "october"
    calendar.November -> "november"
    calendar.December -> "december"
  }
}

fn weekday_name(day: weekday.Weekday) -> String {
  case day {
    weekday.Monday -> "monday"
    weekday.Tuesday -> "tuesday"
    weekday.Wednesday -> "wednesday"
    weekday.Thursday -> "thursday"
    weekday.Friday -> "friday"
    weekday.Saturday -> "saturday"
    weekday.Sunday -> "sunday"
  }
}

fn weekday_name_short(day: weekday.Weekday) -> String {
  case day {
    weekday.Monday -> "mon"
    weekday.Tuesday -> "tue"
    weekday.Wednesday -> "wed"
    weekday.Thursday -> "thu"
    weekday.Friday -> "fri"
    weekday.Saturday -> "sat"
    weekday.Sunday -> "sun"
  }
}

pub fn duration(duration: duration.Duration, _lang: Language) -> Translation {
  let #(hours, minutes) = duration.to_hours_and_minutes(duration)

  let min = int.to_string(minutes) <> "min"

  case hours {
    0 -> Translation(min)
    _ -> Translation(int.to_string(hours) <> "h " <> min)
  }
}

/// Get the translation for a duration in hours as a decimal value. E.g. "2.75"
pub fn as_hours_decimal(
  duration: duration.Duration,
  _lang: Language,
) -> Translation {
  int.to_float(duration.value) /. int.to_float(60)
  |> float.to_precision(2)
  |> float.to_string()
  |> Translation
}
