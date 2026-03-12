import domain/registration
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

  let expected = registration.ActiveRegistration(12, 12, option.None)
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

  let expected = registration.ActiveRegistration(12, 4, option.None)
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

pub fn should_not_initiate_registration_on_update_test() {
  // The registration must have been started first.
  let updated =
    model.initial_model()
    |> webapp.update(message.UpdateRegistering(12))
    |> first

  should.be_none(updated.active_registration)
}
