import app
import gleeunit/should
import message
import model
import test_util.{first}

pub fn should_extend_start_of_day_to_one_hour_earlier_test() {
  let updated =
    model.initial_model()
    |> app.update(message.ExtendStartOfDay)
    |> first

  should.equal(updated.start_of_day, 5)
}

pub fn should_not_extend_start_of_day_beyond_00_test() {
  let updated =
    model.Model(..model.initial_model(), start_of_day: 0)
    |> app.update(message.ExtendStartOfDay)
    |> first

  should.equal(updated.start_of_day, 0)
}

pub fn should_extend_end_of_day_to_one_hour_later_test() {
  let updated =
    model.Model(..model.initial_model(), end_of_day: 18)
    |> app.update(message.ExtendEndOfDay)
    |> first

  should.equal(updated.end_of_day, 19)
}

pub fn should_not_extend_end_of_day_beyond_23_test() {
  let updated =
    model.Model(..model.initial_model(), end_of_day: 23)
    |> app.update(message.ExtendEndOfDay)
    |> first

  should.equal(updated.end_of_day, 23)
}
