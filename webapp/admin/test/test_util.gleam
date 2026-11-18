import gleam/list
import gleam/option
import model

/// Get the first element from a tuple
pub fn first(t: #(a, b)) {
  t.0
}

pub fn get_dialog_value(
  m: model.Model,
  field_id: String,
) -> option.Option(String) {
  // Get the top most dialog and see if the state contains the given field.
  case list.last(m.dialogs), field_id {
    Ok(model.AddUserDialog(state)), "email" ->
      option.Some(state.email.value.value)
    _, _ -> option.None
  }
}
