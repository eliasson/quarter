import gleeunit/should
import model.{initial_model}

pub fn should_have_unauthenticated_user_test() {
  let model = initial_model()

  should.be_false(model.is_authenticated)
}
