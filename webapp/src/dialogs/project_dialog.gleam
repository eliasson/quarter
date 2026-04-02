import domain/color
import domain/input_value.{type InputValue}
import domain/project
import gleam/string
import types.{type FormValue}

pub type State {
  State(
    name: InputValue(String),
    description: InputValue(String),
    color: InputValue(color.Color),
    is_valid: Bool,
  )
}

/// Create the dialog state for creating a new project. I.e. an empty state.
/// The color is set to a random major color.
pub fn new() -> State {
  State(
    input_value.ValidValue(""),
    input_value.ValidValue(""),
    input_value.ValidValue(color.random_major_color()),
    False,
  )
}

/// Create the dialog state for editing a project.
pub fn edit(project: project.Project) -> State {
  State(
    input_value.ValidValue(project.name),
    input_value.ValidValue(project.description),
    input_value.ValidValue(project.color),
    False,
  )
}

pub fn update(state: State, value: FormValue) -> State {
  case value.name {
    "name" ->
      validate(State(..state, name: input_value.ValidValue(value.value)))
    "description" ->
      validate(State(..state, description: input_value.ValidValue(value.value)))
    "color" -> {
      case color.from_hex(value.value) {
        Ok(c) -> validate(State(..state, color: input_value.ValidValue(c)))
        Error(_) ->
          validate(
            State(
              ..state,
              color: input_value.InvalidValue(color.Color(0, 0, 0), [
                "Invalid color",
              ]),
            ),
          )
      }
    }
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

  let color_valid = case state.color {
    input_value.ValidValue(_) -> True
    _ -> False
  }

  State(..state, is_valid: name_valid && description_valid && color_valid)
}
