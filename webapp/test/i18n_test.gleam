import gleam/time/calendar
import gleam/time/timestamp
import gleeunit/should
import i18n

pub fn should_describe_translation_test() {
  i18n.Translation("hello")
  |> i18n.describe
  |> should.equal("hello")
}

pub fn should_capitalize_translation_test() {
  i18n.Translation("hello")
  |> i18n.capitalize
  |> should.equal("Hello")
}

pub fn should_translate_month_test() {
  let t =
    timestamp.parse_rfc3339("2022-01-01T00:00:00Z")
    |> should.be_ok
    |> i18n.name_of_month(i18n.English)

  should.equal(t.value, "january")
}

pub fn should_translate_year_test() {
  let t =
    timestamp.parse_rfc3339("2022-01-01T00:00:00Z")
    |> should.be_ok
    |> i18n.year(i18n.English)

  should.equal(t.value, "2022")
}

pub fn should_translate_day_test() {
  let t =
    timestamp.parse_rfc3339("2022-02-04T00:00:00Z")
    |> should.be_ok
    |> i18n.day(i18n.English)

  should.equal(t.value, "4")
}

pub fn should_translate_monday_test() {
  // January 1, 2024 was a Monday
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 1),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.describe
  |> should.equal("monday")
}

pub fn should_translate_tuesday_test() {
  // January 2, 2024 was a Tuesday
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 2),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.describe
  |> should.equal("tuesday")
}

pub fn should_translate_wednesday_test() {
  // January 3, 2024 was a Wednesday
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 3),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.describe
  |> should.equal("wednesday")
}

pub fn should_translate_thursday_test() {
  // January 4, 2024 was a Thursday
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 4),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.describe
  |> should.equal("thursday")
}

pub fn should_translate_friday_test() {
  // January 5, 2024 was a Friday
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 5),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.describe
  |> should.equal("friday")
}

pub fn should_translate_saturday_test() {
  // January 6, 2024 was a Saturday
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 6),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.describe
  |> should.equal("saturday")
}

pub fn should_translate_sunday_test() {
  // January 7, 2024 was a Sunday
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 7),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.describe
  |> should.equal("sunday")
}

pub fn should_translate_day_with_capitalize_test() {
  // Test that capitalization works with weekdays
  let ts =
    timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 1),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  ts
  |> i18n.name_of_day(i18n.English)
  |> i18n.capitalize
  |> should.equal("Monday")
}
