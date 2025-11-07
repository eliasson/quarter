import gleam/list
import listext
import route
import user

pub type Model {
  Model(
    is_authenticated: Bool,
    route: route.Route,
    /// The open drop down menus, a list to allow for nested menus.
    dropdowns: List(DropDownMenu),
    /// The list of all system users if loaded (for admin).
    users: List(user.User),
  )
}

pub type DropDownMenu {
  DropDownMenu(id: String)
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(is_authenticated: False, route: route.Home, dropdowns: [], users: [])
}

pub fn navigate_to(m: Model, route: route.Route) -> Model {
  Model(..m, route: route)
}

pub fn open_main_menu(m: Model) -> Model {
  m |> close_all_drop_downs |> open_drop_down(DropDownMenu("main.nav"))
}

/// Close the top most modal (e.g. drop-down menu or dialog).
pub fn close_modal(m: Model) {
  let dropdowns = listext.drop_last(m.dropdowns)
  Model(..m, dropdowns: dropdowns)
}

pub fn set_current_user(m: Model, _user: user.User) {
  Model(..m, is_authenticated: True)
}

pub fn set_users(m: Model, users: List(user.User)) {
  Model(..m, users: users)
}

fn open_drop_down(m: Model, menu: DropDownMenu) {
  Model(..m, dropdowns: list.append(m.dropdowns, [menu]))
}

fn close_all_drop_downs(m: Model) -> Model {
  Model(..m, dropdowns: [])
}
