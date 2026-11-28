import dialogs/user_dialog
import domain
import gleam/list
import gleam/option
import quarter_shared/domain/user
import quarter_shared/types
import quarter_shared/util/seq
import route

pub type Model {
  Model(
    route: route.Route,
    /// The logged in user, once loaded.
    current_user: option.Option(user.User),
    /// The list of all system users.
    users: List(domain.UserListItem),
    /// The open drop down menus, a list to allow for nested menus.
    dropdowns: List(DropDownMenu),
    /// The open dialogs as a reverse stack (top most is last in list).
    dialogs: List(Dialog),
    /// The errors that have occured and that are not dismissed
    errors: List(ApplicationError),
  )
}

pub type DropDownMenu {
  DropDownMenu(id: String)
}

pub type Dialog {
  AddUserDialog(state: user_dialog.State)
}

/// An application error is to be displayed for the user when something
/// goes wrong. These are currently not actionable, just intended as an FYI.
pub type ApplicationError {
  ApplicationError(id: String, message: String)
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(
    route: route.Users,
    current_user: option.None,
    users: [],
    dropdowns: [],
    dialogs: [],
    errors: [],
  )
}

pub fn navigate_to(m: Model, route: route.Route) -> Model {
  Model(..m, route:)
}

pub fn set_current_user(m: Model, user: user.User) -> Model {
  Model(..m, current_user: option.Some(user))
}

pub fn set_users(m: Model, users: List(domain.UserListItem)) -> Model {
  Model(..m, users:)
}

pub fn open_drop_down_menu(m: Model, id: String) -> Model {
  Model(..m, dropdowns: list.append(m.dropdowns, [DropDownMenu(id)]))
}

pub fn is_drop_down_menu_open(m: Model, id: String) -> Bool {
  list.any(m.dropdowns, fn(dd) { dd.id == id })
}

/// Close the top most modal (e.g. drop-down menu or dialog).
pub fn close_modal(m: Model) -> Model {
  Model(
    ..m,
    dropdowns: seq.drop_last(m.dropdowns),
    dialogs: seq.drop_last(m.dialogs),
  )
}

/// Close all modals (e.g. drop-down menus and dialogs).
pub fn close_all_modals(m: Model) -> Model {
  Model(..m, dropdowns: [], dialogs: [])
}

pub fn open_dialog(m: Model, dialog: Dialog) -> Model {
  Model(..m, dropdowns: [], dialogs: list.append(m.dialogs, [dialog]))
}

pub fn current_dialog(m: Model) -> Result(Dialog, Nil) {
  list.last(m.dialogs)
}

pub fn update_dialog_value(m: Model, value: types.FormValue) -> Model {
  let updated_dialogs = case list.last(m.dialogs) {
    Ok(AddUserDialog(state)) -> [
      AddUserDialog(user_dialog.update(state, value)),
    ]
    Error(_) -> []
  }

  let dialogs =
    seq.drop_last(m.dialogs)
    |> list.append(updated_dialogs)

  Model(..m, dialogs:)
}

pub fn new_user_dialog() -> Dialog {
  AddUserDialog(user_dialog.new())
}

pub fn add_error(m: Model, error: ApplicationError) -> Model {
  Model(..m, errors: list.append(m.errors, [error]))
}

pub fn dismiss_error(m: Model, id: String) -> Model {
  Model(..m, errors: list.filter(m.errors, fn(e) { e.id != id }))
}
