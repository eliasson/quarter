/// A value emitted from a forms input control. E.g. when the user enters text in a <input> element.
pub type FormValue {
  /// The name of the input element and its current value (always string, regardless of input type attribute).
  FormValue(name: String, value: String)
}
