import gleam/list
import gleam/option
import listext
import route
import user
import util.{type Email}

pub type Model {
  Model(
    is_authenticated: Bool,
    route: route.Route,
    /// The open drop down menus, a list to allow for nested menus.
    dropdowns: List(DropDownMenu),
    /// The open dialogs as a reverse stack (top most is last in list).
    dialogs: List(Dialog),
    /// The list of all system users if loaded (for admin).
    users: List(user.User),
    /// The errors that have occured and that are not dismissed
    errors: List(ApplicationError),
  )
}

pub type DropDownMenu {
  DropDownMenu(id: String)
}

pub type Dialog {
  AddUserDialog(state: UserDialogState)
  AnotherDialog(state: AnotherDialogState)
}

pub type UserDialogState {
  UserDialogState(email: Email, email_errors: List(String), is_valid: Bool)
}

pub type AnotherDialogState {
  AnotherDialogState(foo: String)
}

/// An application error is to be displayed for the user when something
/// goes wrong. These are currently not actionable, just intended as an FYI.
pub type ApplicationError {
  ApplicationError(id: String, message: String)
}

pub type FormValue {
  FormValue(name: String, value: String)
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(
    is_authenticated: False,
    route: route.Home,
    dropdowns: [],
    dialogs: [],
    users: [],
    errors: [],
  )
}

pub fn navigate_to(m: Model, route: route.Route) -> Model {
  Model(..m, route: route)
}

pub fn open_drop_down_menu(m: Model, id: String) -> Model {
  Model(..m, dropdowns: list.append(m.dropdowns, [DropDownMenu(id)]))
}

pub fn is_drop_down_menu_open(m: Model, id: String) -> Bool {
  case list.find(m.dropdowns, fn(dd) { dd.id == id }) {
    Ok(_) -> True
    _ -> False
  }
}

/// Close the top most modal (e.g. drop-down menu or dialog).
pub fn close_modal(m: Model) {
  // TODO: How to find the top most?
  let dropdowns = listext.drop_last(m.dropdowns)
  let dialogs = listext.drop_last(m.dialogs)

  Model(..m, dropdowns: dropdowns, dialogs: dialogs)
}

/// Close all modals (e.g. drop-down menus and dialogs).
pub fn close_all_modals(m: Model) {
  Model(..m, dropdowns: [])
}

pub fn set_current_user(m: Model, _user: user.User) {
  Model(..m, is_authenticated: True)
}

pub fn set_users(m: Model, users: List(user.User)) {
  Model(..m, users: users)
}

pub fn open_dialog(m: Model, dialog: Dialog) -> Model {
  Model(..m, dropdowns: [], dialogs: list.append(m.dialogs, [dialog]))
}

pub fn add_error(m: Model, error: ApplicationError) {
  Model(..m, errors: list.append(m.errors, [error]))
}

pub fn dismiss_error(m: Model, id: String) {
  let errors = list.filter(m.errors, fn(e) { e.id != id })
  Model(..m, errors:)
}

pub fn get_dialog_value(m: Model, field_id: String) -> option.Option(String) {
  // Get the top most dialog and see if the state contains the given field.
  let value = case list.last(m.dialogs) {
    Ok(d) -> {
      case d {
        AddUserDialog(state) -> {
          case field_id {
            "email" -> option.Some(state.email.value)
            _ -> option.None
          }
        }
        _ -> {
          option.None
        }
      }
    }
    _ -> option.None
  }
  value
}

pub fn update_dialog_value(m: Model, value: FormValue) -> Model {
  // Get the last dialog
  // Update its state
  let updated_dialogs = case list.last(m.dialogs) {
    Ok(d) -> {
      case d {
        AddUserDialog(state) -> {
          let updated_state = case value.name {
            "email" ->
              validate_user_dialog_state(
                UserDialogState(..state, email: util.Email(value.value)),
              )
            _ -> state
          }

          [AddUserDialog(state: updated_state)]
        }
        _ -> [d]
      }
    }
    _ -> []
  }

  // Replace the dialog in the list of dialog
  let dialogs =
    listext.drop_last(m.dialogs)
    |> list.append(updated_dialogs)

  Model(..m, dialogs:)
}

pub fn validate_user_dialog_state(state: UserDialogState) -> UserDialogState {
  // Validate each field and add approrpiate error messages

  // Get validation errors for email
  // Set the state is_valid based on if there are _any_ validation messages.
  state
}

pub fn new_user_dialog() {
  AddUserDialog(UserDialogState(util.Email(""), [], False))
}
