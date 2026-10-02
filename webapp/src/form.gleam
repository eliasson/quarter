pub type Form(msg) {
  Form(id: String, fields: List(FormField), actions: List(FormAction(msg)))
}

pub type FormField {
  /// Displays a text message.
  TextMessage(message: String)

  /// Text input field with email validation.
  EmailInput(
    name: String,
    label: String,
    value: String,
    required: Bool,
    autofocus: Bool,
  )

  /// Text input field
  TextInput(
    name: String,
    label: String,
    value: String,
    required: Bool,
    autofocus: Bool,
  )

  /// Text area input field
  TextAreaInput(
    name: String,
    label: String,
    value: String,
    required: Bool,
    autofocus: Bool,
  )

  /// Color input field
  ColorInput(
    name: String,
    label: String,
    value: String,
    required: Bool,
    autofocus: Bool,
  )
}

pub type FormAction(msg) {
  Cancel(msg: msg)
  Confirm(disabled: Bool, msg: msg)
}
