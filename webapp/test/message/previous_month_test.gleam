import gleam/time/calendar
import gleeunit/should
import message
import model
import test_util.{first, new_timestamp}
import webapp

pub fn should_update_today_to_previous_month_test() {
  let updated =
    model.Model(
      ..model.initial_model(),
      today: new_timestamp(2023, calendar.January, 12),
    )
    |> webapp.update(message.PreviousMonth)
    |> first

  should.equal(updated.today, new_timestamp(2022, calendar.December, 1))
}
