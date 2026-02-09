/// Our homebrewd i18n solution.
///
/// Keep i18n minimal and preferrably type safe. The idea is that this module will deal with
/// all translations and internationalization, not any type specific module.
///
/// Quarter uses relativly few translations (I think), so we can keep it this simple.
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

pub fn name_of_month(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  Translation(calendar_month(date.month))
}

pub fn name_of_day(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let weekday = weekday.from_timestamp(ts)
  Translation(weekday_name(weekday))
}

pub fn year(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  Translation(int.to_string(date.year))
}

pub fn day(ts: timestamp.Timestamp, _lang: Language) -> Translation {
  let #(date, _time) = timestamp.to_calendar(ts, calendar.utc_offset)
  Translation(int.to_string(date.day))
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
