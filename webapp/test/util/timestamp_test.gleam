import gleam/list
import gleam/time/calendar
import gleam/time/timestamp as std_timestamp
import gleeunit/should
import util/timestamp

pub fn should_convert_timestamp_to_iso_date_test() {
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 8),
      time: calendar.TimeOfDay(12, 30, 50, 0),
      offset: calendar.utc_offset,
    )

  timestamp.to_iso_date(ts)
  |> should.equal("2026-02-08")
}

pub fn should_zero_time_part_of_timestamp_test() {
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 8),
      time: calendar.TimeOfDay(12, 30, 50, 0),
      offset: calendar.utc_offset,
    )
  let expected =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 8),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  timestamp.with_zero_time(ts)
  |> should.equal(expected)
}

pub fn should_convert_iso_string_to_timestamp_test() {
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 8),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  timestamp.to_iso_date(ts)
  |> timestamp.from_iso_date
  |> should.be_ok
  |> should.equal(ts)
}

pub fn next_first_of_month_test() {
  let tests = [
    #(calendar.Date(2024, calendar.January, 15), "2024-02-01"),
    #(calendar.Date(2024, calendar.December, 31), "2025-01-01"),
  ]

  list.each(tests, fn(t) {
    let ts =
      std_timestamp.from_calendar(
        date: t.0,
        time: calendar.TimeOfDay(12, 30, 50, 0),
        offset: calendar.utc_offset,
      )

    timestamp.next_first_of_month(ts)
    |> timestamp.to_iso_date()
    |> should.equal(t.1)
  })
}

pub fn previous_first_of_month_test() {
  let tests = [
    #(calendar.Date(2024, calendar.January, 15), "2023-12-01"),
    #(calendar.Date(2024, calendar.January, 1), "2023-12-01"),
    #(calendar.Date(2024, calendar.October, 1), "2024-09-01"),
  ]

  list.each(tests, fn(t) {
    let ts =
      std_timestamp.from_calendar(
        date: t.0,
        time: calendar.TimeOfDay(12, 30, 50, 0),
        offset: calendar.utc_offset,
      )

    timestamp.previous_first_of_month(ts)
    |> timestamp.to_iso_date()
    |> should.equal(t.1)
  })
}

pub fn tomorrow_test() {
  let tests = [
    #(calendar.Date(2024, calendar.January, 15), "2024-01-16"),
    #(calendar.Date(2024, calendar.January, 31), "2024-02-01"),
    #(calendar.Date(2024, calendar.February, 28), "2024-02-29"),
    #(calendar.Date(2024, calendar.February, 29), "2024-03-01"),
    #(calendar.Date(2024, calendar.December, 31), "2025-01-01"),
  ]

  list.each(tests, fn(t) {
    let ts =
      std_timestamp.from_calendar(
        date: t.0,
        time: calendar.TimeOfDay(12, 30, 50, 0),
        offset: calendar.utc_offset,
      )

    timestamp.tomorrow(ts)
    |> timestamp.to_iso_date()
    |> should.equal(t.1)
  })
}

pub fn iso_week_number_test() {
  let tests = [
    // Normal week in the middle of the year
    #(calendar.Date(2026, calendar.March, 22), 12),
    // Jan 1 that falls in week 1 of its own year (Thursday)
    #(calendar.Date(2026, calendar.January, 1), 1),
    // Week 53 — year with 53 weeks (2026 starts on Thursday)
    #(calendar.Date(2026, calendar.December, 28), 53),
    // Dec 31 that falls in week 1 of the next year
    #(calendar.Date(2025, calendar.December, 31), 1),
    // Jan 1 that falls in week 53 of the previous year (2016 starts on Friday)
    #(calendar.Date(2016, calendar.January, 1), 53),
    // Regular mid-year week
    #(calendar.Date(2024, calendar.June, 17), 25),
  ]

  list.each(tests, fn(t) {
    let ts =
      std_timestamp.from_calendar(
        date: t.0,
        time: calendar.TimeOfDay(12, 0, 0, 0),
        offset: calendar.utc_offset,
      )

    timestamp.iso_week_number(ts)
    |> should.equal(t.1)
  })
}

pub fn yesterday_test() {
  let tests = [
    #(calendar.Date(2024, calendar.January, 15), "2024-01-14"),
    #(calendar.Date(2024, calendar.February, 1), "2024-01-31"),
    #(calendar.Date(2024, calendar.March, 1), "2024-02-29"),
    #(calendar.Date(2025, calendar.January, 1), "2024-12-31"),
  ]

  list.each(tests, fn(t) {
    let ts =
      std_timestamp.from_calendar(
        date: t.0,
        time: calendar.TimeOfDay(12, 30, 50, 0),
        offset: calendar.utc_offset,
      )

    timestamp.yesterday(ts)
    |> timestamp.to_iso_date()
    |> should.equal(t.1)
  })
}

pub fn is_same_date_test() {
  let one =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 8),
      time: calendar.TimeOfDay(12, 30, 50, 0),
      offset: calendar.utc_offset,
    )
  let two =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 8),
      time: calendar.TimeOfDay(13, 33, 33, 0),
      offset: calendar.utc_offset,
    )

  timestamp.is_same_date(one, two)
  |> should.be_true

  timestamp.is_same_date(two, one)
  |> should.be_true
}

pub fn is_not_same_date_test() {
  let one =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 8),
      time: calendar.TimeOfDay(12, 30, 50, 0),
      offset: calendar.utc_offset,
    )

  let two =
    std_timestamp.from_calendar(
      date: calendar.Date(2026, calendar.February, 9),
      time: calendar.TimeOfDay(12, 30, 50, 0),
      offset: calendar.utc_offset,
    )

  timestamp.is_same_date(one, two)
  |> should.be_false

  timestamp.is_same_date(two, one)
  |> should.be_false
}
