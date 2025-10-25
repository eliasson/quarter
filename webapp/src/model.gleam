pub type Model {
  Model(is_authenticated: Bool)
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(is_authenticated: False)
}
