import message

pub type Form {
  Form(id: String, fields: List(FormField), actions: List(FormAction))
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
}

pub type FormAction {
  Cancel
  Confirm(disabled: Bool, msg: message.Msg)
}
