import domain/input_value.{type InputValue}
import domain/project
import gleam/string
import types.{type FormValue}

pub type State {
  State(
    name: InputValue(String),
    description: InputValue(String),
    is_valid: Bool,
  )
}

/// Create the dialog state for creating a new project. I.e. an empty state.
pub fn new() -> State {
  State(input_value.ValidValue(""), input_value.ValidValue(""), False)
}

/// Create the dialog state for editing a project.
pub fn edit(project: project.Project) -> State {
  State(
    input_value.ValidValue(project.name),
    input_value.ValidValue(project.description),
    False,
  )
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
  let name_valid = case state.name {
    input_value.ValidValue(name) -> !string.is_empty(name)
    _ -> False
  }

  let description_valid = case state.description {
    input_value.ValidValue(_) -> True
    _ -> False
  }

  State(..state, is_valid: name_valid && description_valid)
}
