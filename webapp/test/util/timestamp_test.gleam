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
