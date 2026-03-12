import gleam/option
import gleam/time/calendar
import gleeunit/should
import message
import model
import test_util.{first, new_timestamp}
import webapp

pub fn should_update_today_to_tomorrow_test() {
  let sheet =
    test_util.new_timesheet("2026-02-07T18:00:00Z", [])
    |> should.be_ok

  let updated =
    model.Model(
      ..model.initial_model(),
      today: new_timestamp(2023, calendar.February, 20),
    )
    |> webapp.update(message.TimesheetResult(Ok(sheet)))
    |> first

  should.equal(updated.active_timesheet, option.Some(sheet))
}
