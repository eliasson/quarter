import domain/input_value.{type InputValue}
import gleam/string
import types.{type FormValue}

pub type State {
  State(
    name: InputValue(String),
    description: InputValue(String),
    is_valid: Bool,
  )
}

pub fn new() -> State {
  State(input_value.ValidValue(""), input_value.ValidValue(""), False)
}

pub fn update(state: State, value: FormValue) -> State {
  case value.name {
    "name" ->
      validate(State(..state, name: input_value.ValidValue(value.value)))
    "description" ->
      validate(State(..state, description: input_value.ValidValue(value.value)))
    _ -> state
  }
}

pub fn validate(state: State) -> State {
  let is_valid = case state.name {
    input_value.ValidValue(name) -> !string.is_empty(name)
    _ -> False
  }

  State(..state, is_valid:)
}
