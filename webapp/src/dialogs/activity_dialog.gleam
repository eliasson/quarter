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

/// Create the dialog state for creating a new activity. I.e. an empty state.
pub fn new() -> State {
  State(
    input_value.ValidValue(""),
    input_value.ValidValue(""),
    input_value.ValidValue(color.Color(142, 135, 245)),
    False,
  )
}

/// Create the dialog state for editing an activity.
pub fn edit(activity: project.Activity) -> State {
  State(
    input_value.ValidValue(activity.name),
    input_value.ValidValue(activity.description),
    input_value.ValidValue(activity.color),
    True,
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
    input_value.ValidValue(description) -> !string.is_empty(description)
    _ -> False
  }

  let color_valid = case state.color {
    input_value.ValidValue(_) -> True
    _ -> False
  }

  State(..state, is_valid: name_valid && description_valid && color_valid)
}
