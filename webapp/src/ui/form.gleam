import gleam/list
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/event
import message
import ui/core as ui

pub type Form {
  Form(id: String, fields: List(FormField), actions: List(FormAction))
}

pub type FormField {
  EmailInput(name: String, label: String, value: String, required: Bool)
}

pub type FormAction {
  Cancel
  Confirm
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
      att.type_(button_type),
      att.disabled(disabled),
      event.on_click(handle_click),
    ],
    [
      html.div([att.class("button")], [
        html.span([], [html.text(label)]),
      ]),
    ],
  )
}

pub fn ghost_button(icon ico: String) {
  html.button([att.class("ghost")], [ui.icon(ico, ui.MediumSize)])
}

pub fn outline_button(text: String, icon ico: String) {
  html.button([att.class("ghost")], [
    html.text(text),
    ui.icon(ico, ui.SmallSize),
  ])
}

/// A form containing a dialog.
pub fn form_dialog(form: Form, ico: String, header: String) {
  let content = list.map(form.fields, fn(f) { render_field(form.id, f) })
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

fn render_field(
  form_id id: String,
  field field: FormField,
) -> element.Element(message.Msg) {
  case field {
    EmailInput(name, label, value, required) ->
      input_field(id, "email", name, label, value, required)
  }
}

fn render_action(action action: FormAction) -> element.Element(message.Msg) {
  case action {
    Cancel -> button("cancel", "Cancel", False, message.CloseModal)
    Confirm -> button("submit", "Confirm", False, message.ConfirmDialog)
  }
}

fn input_field(
  form_id: String,
  input_type: String,
  name: String,
  label: String,
  inital_value: String,
  required: Bool,
) -> element.Element(message.Msg) {
  html.fieldset([], [
    html.label([att.for(name)], [html.text(label)]),
    html.input([
      att.type_(input_type),
      att.name(name),
      att.value(inital_value),
      att.required(required),
      event.on_input(fn(updated_value) {
        message.FormTextFieldUpdated(
          form_id,
          message.FormValue(name, updated_value),
        )
      }),
    ]),
  ])
}
