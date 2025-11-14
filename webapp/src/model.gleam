import gleam/list
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
  )
}

pub type DropDownMenu {
  DropDownMenu(id: String)
}

pub type Dialog {
  AddUserDialog(email: Email)
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(
    is_authenticated: False,
    route: route.Home,
    dropdowns: [],
    dialogs: [],
    users: [],
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
