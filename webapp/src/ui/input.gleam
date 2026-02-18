import form
import gleam/list
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/event
import message
import types
import ui/core as ui

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
      ui.click_stop(handle_click),
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
      ui.click_stop(handle_click),
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

/// A dialog containing a form.
pub fn form_dialog(form: form.Form, ico: String, header: String) {
  let content = list.map(form.fields, fn(f) { render_field(f) })
  let actions = list.map(form.actions, fn(f) { render_action(f) })

  html.div([att.class("dialog-backdrop")], [
    html.div(
      [
        att.class("dialog-container"),
        att.role("dialog"),
        att.aria_modal(True),
      ],
      [
        html.div([att.class("dialog-header")], [
          html.span([], [html.text(header)]),
        ]),
        html.div([att.class("dialog-content")], content),
        html.div([att.class("dialog-footer")], actions),
      ],
    ),
  ])
}

fn render_field(field field: form.FormField) -> element.Element(message.Msg) {
  case field {
    form.EmailInput(name, label, value, required, autofocus) ->
      input_field("email", name, label, value, required, autofocus)
    form.TextInput(name, label, value, required, autofocus) ->
      input_field("text", name, label, value, required, autofocus)
    form.TextAreaInput(name, label, value, required, autofocus) ->
      text_area(name, label, value, required, autofocus)
    form.ColorInput(name, label, value, required, autofocus) ->
      input_field("color", name, label, value, required, autofocus)
    form.TextMessage(text) -> text_message(text)
  }
}

fn render_action(action action: form.FormAction) -> element.Element(message.Msg) {
  case action {
    form.Cancel -> cancel_button(message.CloseModal)
    form.Confirm(disabled, msg) -> button("submit", "Confirm", disabled, msg)
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
          message.FormTextFieldUpdated(types.FormValue(name, updated_value))
        }),
      ],
      validations,
    )),
  ])
}

fn text_area(
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
    html.textarea(
      list.append(
        [
          att.name(name),
          att.autofocus(autofocus),
          att.rows(6),
          event.on_input(fn(updated_value) {
            message.FormTextFieldUpdated(types.FormValue(name, updated_value))
          }),
        ],
        validations,
      ),
      inital_value,
    ),
  ])
}

fn text_message(text: String) {
  html.p([], [html.text(text)])
}
