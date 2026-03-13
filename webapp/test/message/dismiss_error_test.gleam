import gleam/list
import gleeunit/should
import message
import model
import test_util.{first}
import webapp

pub fn should_remove_the_error_from_list_of_errors_test() {
  let m =
    model.initial_model()
    |> model.add_error(model.ApplicationError("1", "Oh no!"))
    |> model.add_error(model.ApplicationError("2", "Jikes"))
    |> webapp.update(message.DismissError("2"))
    |> first

  let remaining = list.map(m.errors, fn(e) { e.id })

  should.equal(remaining, ["1"])
}
