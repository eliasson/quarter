import route

pub type Model {
  Model(is_authenticated: Bool, route: route.Route)
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(is_authenticated: False, route: route.Home)
}

pub fn navigate_to(m: Model, route: route.Route) -> Model {
  Model(..m, route: route)
}
