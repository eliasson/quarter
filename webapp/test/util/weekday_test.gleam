import gleam/list
import gleam/time/calendar
import gleam/time/timestamp as std_timestamp
import gleeunit/should
import util/weekday

pub fn weekday_test() {
  let tests = [
    #(calendar.Date(2024, calendar.January, 1), weekday.Monday),
    #(calendar.Date(2024, calendar.January, 2), weekday.Tuesday),
    #(calendar.Date(2024, calendar.January, 3), weekday.Wednesday),
    #(calendar.Date(2024, calendar.January, 4), weekday.Thursday),
    #(calendar.Date(2024, calendar.January, 5), weekday.Friday),
    #(calendar.Date(2024, calendar.January, 6), weekday.Saturday),
    #(calendar.Date(2024, calendar.January, 7), weekday.Sunday),
  ]

  list.each(tests, fn(t) {
    let ts =
      std_timestamp.from_calendar(
        date: t.0,
        time: calendar.TimeOfDay(12, 0, 0, 0),
        offset: calendar.utc_offset,
      )

    weekday.from_timestamp(ts)
    |> should.equal(t.1)
  })
}

pub fn unix_epoch_test() {
  // January 1, 1970 was a Thursday
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(1970, calendar.January, 1),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  weekday.from_timestamp(ts)
  |> should.equal(weekday.Thursday)
}

pub fn year_2000_test() {
  // January 1, 2000 was a Saturday
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2000, calendar.January, 1),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  weekday.from_timestamp(ts)
  |> should.equal(weekday.Saturday)
}

pub fn leap_year_test() {
  // February 29, 2020 was a Saturday
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2020, calendar.February, 29),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  weekday.from_timestamp(ts)
  |> should.equal(weekday.Saturday)
}

pub fn midnight_test() {
  // January 1, 2024 at midnight was still a Monday
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 1),
      time: calendar.TimeOfDay(0, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  weekday.from_timestamp(ts)
  |> should.equal(weekday.Monday)
}

pub fn end_of_day_test() {
  // January 1, 2024 at 23:59:59 was still a Monday
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2024, calendar.January, 1),
      time: calendar.TimeOfDay(23, 59, 59, 0),
      offset: calendar.utc_offset,
    )

  weekday.from_timestamp(ts)
  |> should.equal(weekday.Monday)
}

pub fn december_test() {
  // December 25, 2023 was a Monday
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2023, calendar.December, 25),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  weekday.from_timestamp(ts)
  |> should.equal(weekday.Monday)
}

pub fn future_date_test() {
  // January 1, 2030 will be a Tuesday
  let ts =
    std_timestamp.from_calendar(
      date: calendar.Date(2030, calendar.January, 1),
      time: calendar.TimeOfDay(12, 0, 0, 0),
      offset: calendar.utc_offset,
    )

  weekday.from_timestamp(ts)
  |> should.equal(weekday.Tuesday)
}
