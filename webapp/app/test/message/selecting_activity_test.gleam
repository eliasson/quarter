import app
import gleam/option
import gleeunit/should
import message
import model
import test_util.{first}

pub fn should_update_model_with_activity_test() {
  let activity = test_util.arbitrary_activity()

  let updated =
    model.initial_model()
    |> app.update(message.SelectActivity(option.Some(activity)))
    |> first

  should.equal(updated.selected_activity, option.Some(activity))
}
