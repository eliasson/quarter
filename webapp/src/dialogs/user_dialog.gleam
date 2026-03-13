import domain/email
import domain/input_value.{type InputValue}
import gleam/list
import types.{type FormValue}

pub type State {
  State(email: InputValue(email.Email), is_valid: Bool)
}

pub fn new() -> State {
  State(input_value.ValidValue(email.Email("")), False)
}

pub fn update(state: State, value: FormValue) -> State {
  case value.name {
    "email" ->
      validate(
        State(..state, email: input_value.ValidValue(email.Email(value.value))),
      )
    _ -> state
  }
}

pub fn validate(state: State) -> State {
  // Validate each field and add approrpiate error messages

  // Get validation errors for email
  let email = case email.validate_email(state.email.value) {
    Ok(Nil) -> input_value.ValidValue(state.email.value)
    Error(messages) -> input_value.InvalidValue(state.email.value, messages)
  }

  // Set the state is_valid based on if there are _any_ validation messages.
  let has_error =
    list.any([email], fn(v) {
      case v {
        input_value.ValidValue(_) -> False
        _ -> True
      }
    })

  State(email:, is_valid: !has_error)
}
