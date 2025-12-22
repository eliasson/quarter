import gleam/list
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/event
import message
import model
import ui/core as ui

pub type Form {
  Form(id: String, fields: List(FormField), actions: List(FormAction))
}

pub type FormField {
  EmailInput(
    name: String,
    label: String,
    value: String,
    required: Bool,
    autofocus: Bool,
  )
}

pub type FormAction {
  Cancel
  Confirm(disabled: Bool)
}

pub fn checkbox() -> element.Element(msg) {
  html.label([att.class("checkbox")], [html.input([att.type_("checkbox")])])
}

pub fn button(
  button_type: String,
  label: String,
  disabled: Bool,
  on_click handle_click: msg,
) -> element.Element(msg) {
  html.button(
    [
      att.class("button"),
      att.type_(button_type),
      att.disabled(disabled),
      event.on_click(handle_click),
    ],
    [
      html.span([], [html.text(label)]),
    ],
  )
}

pub fn icon_button(
  button_type: String,
  ico: String,
  label: String,
  disabled: Bool,
  on_click handle_click: msg,
) -> element.Element(msg) {
  html.button(
    [
      att.class("button"),
      att.type_(button_type),
      att.disabled(disabled),
      event.on_click(handle_click),
    ],
    [
      ui.icon(ico, ui.SmallSize),
      html.span([], [html.text(label)]),
    ],
  )
}

pub fn cancel_button(on_click handle_click: msg) -> element.Element(msg) {
  html.button(
    [
      att.class("cancel"),
      att.type_("cancel"),
      event.on_click(handle_click),
    ],
    [
      html.div([att.class("button")], [
        html.span([], [html.text("Cancel")]),
      ]),
    ],
  )
}

pub fn ghost_button(ico: String, on_click: msg) {
  html.button(
    [
      att.class("ghost"),
      event.on_click(on_click),
    ],
    [ui.icon(ico, ui.MediumSize)],
  )
}

/// Generate a button, but without any action.
pub fn fake_button(ico: String) {
  html.button(
    [
      att.class("ghost"),
    ],
    [ui.icon(ico, ui.MediumSize)],
  )
}

pub fn outline_button(text: String, icon ico: String) {
  html.button([att.class("ghost")], [
    html.text(text),
    ui.icon(ico, ui.SmallSize),
  ])
}

/// A form containing a dialog.
pub fn form_dialog(form: Form, ico: String, header: String) {
  let content = list.map(form.fields, fn(f) { render_field(f) })
  let actions = list.map(form.actions, fn(f) { render_action(f) })

  html.div([att.class("dialog")], [
    html.div([att.class("dialog-backdrop")], []),
    html.div([att.class("dialog-overlay")], [
      html.div([att.class("dialog-container")], [
        html.div([att.class("dialog-header")], [
          ui.icon(ico, ui.MediumSize),
          html.span([], [html.text(header)]),
        ]),
        html.div([att.class("dialog-content")], content),
        html.div([att.class("dialog-footer")], actions),
      ]),
    ]),
  ])
}

fn render_field(field field: FormField) -> element.Element(message.Msg) {
  case field {
    EmailInput(name, label, value, required, autofocus) ->
      input_field("email", name, label, value, required, autofocus)
  }
}

fn render_action(action action: FormAction) -> element.Element(message.Msg) {
  case action {
    Cancel -> cancel_button(message.CloseModal)
    Confirm(disabled) ->
      button("submit", "Confirm", disabled, message.ConfirmDialog)
  }
}

fn input_field(
  input_type: String,
  name: String,
  label: String,
  inital_value: String,
  required: Bool,
  autofocus: Bool,
) -> element.Element(message.Msg) {
  // Only add the required attribute if the input has a value. Else the field will be invalid immediately.
  let validations = case inital_value {
    "" -> []
    _ -> [att.required(required)]
  }

  html.fieldset([], [
    html.label([att.for(name)], [html.text(label)]),
    html.input(list.append(
      [
        att.type_(input_type),
        att.name(name),
        att.value(inital_value),
        att.autofocus(autofocus),
        event.on_input(fn(updated_value) {
          message.FormTextFieldUpdated(model.FormValue(name, updated_value))
        }),
      ],
      validations,
    )),
  ])
}
