import route

pub type Model {
  Model(is_authenticated: Bool, route: route.Route, menu_main_open: Bool)
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(is_authenticated: False, route: route.Home, menu_main_open: False)
}

pub fn navigate_to(m: Model, route: route.Route) -> Model {
  Model(..m, route: route)
}

pub fn open_main_menu(m: Model) -> Model {
  Model(..m, menu_main_open: True)
}
