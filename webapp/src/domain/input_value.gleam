/// Represents a model value that is inputted by the user.
pub type InputValue(a) {
  UnvalidatedValue(value: a)
  ValidValue(value: a)
  InvalidValue(value: a, errors: List(String))
}
