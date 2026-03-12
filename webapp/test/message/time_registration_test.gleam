import gleam/option
import gleeunit/should
import message
import model
import test_util.{first}
import webapp

pub fn should_create_active_registration_test() {
  let updated =
    model.initial_model()
    |> webapp.update(message.StartRegistering(12))
    |> first

  let expected = model.ActiveRegistration(12, 12)
  should.equal(updated.active_registration, option.Some(expected))
}

pub fn should_update_active_registration_test() {
  let updated =
    model.initial_model()
    |> webapp.update(message.StartRegistering(12))
    |> first
    |> webapp.update(message.UpdateRegistering(8))
    |> first
    |> webapp.update(message.UpdateRegistering(4))
    |> first

  let expected = model.ActiveRegistration(12, 4)
  should.equal(updated.active_registration, option.Some(expected))
}

pub fn should_clear_active_registration_test() {
  let updated =
    model.initial_model()
    |> webapp.update(message.StartRegistering(12))
    |> first
    |> webapp.update(message.CommitRegistering)
    |> first

  should.be_none(updated.active_registration)
}
