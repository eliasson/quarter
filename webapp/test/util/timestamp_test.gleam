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
