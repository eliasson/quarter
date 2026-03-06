import gleam/option
import gleeunit/should
import message
import model
import test_util.{first}
import webapp

pub fn should_update_model_with_activity_test() {
  let id = test_util.arbitrary_activity().id

  let updated =
    model.initial_model()
    |> webapp.update(message.SelectActivity(option.Some(id)))
    |> first

  should.equal(updated.selected_activity, option.Some(id))
}
