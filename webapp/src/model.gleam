pub type Model {
  Model(
    is_authenticated: Bool,
    // Temporary
    counter: Int,
  )
}

/// Creates a new model with the initial fields all set.
pub fn initial_model() -> Model {
  Model(is_authenticated: False, counter: 0)
}
