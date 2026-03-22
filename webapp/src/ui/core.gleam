import gleam/time/timestamp
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/element/svg
import lustre/event
import message
import ui/graphics
import util/timestamp as tsutil

pub type Size {
  SmallSize
  MediumSize
}

pub fn icon(icon_name: String, size: Size) {
  // I did not manage to produce an attribute using xlink:href, but only href
  // seems to work fine in Firefox at least.
  //
  // NOTE lustre/eleemnt have namespaced function, try that!

  let size_class = case size {
    SmallSize -> " small"
    _ -> ""
  }

  html.svg([att.class("icon" <> size_class)], [
    svg.use_([att.attribute("href", "#" <> icon_name)]),
  ])
}

pub fn chip(text: String) {
  html.span([att.class("chip")], [html.text(text)])
}

pub fn timestamp(ts: timestamp.Timestamp) -> element.Element(msg) {
  html.time([], [html.text(tsutil.to_iso_date(ts))])
}

pub fn empty_state(title: String, text: String) -> element.Element(msg) {
  html.div([att.class("empty-state")], [
    html.h2([], [html.text(title)]),
    html.p([att.class("empty-state-text")], [html.text(text)]),
  ])
}

/// Display an error notification with a dismiss button in the corner of the page.
pub fn error_notification(
  id: String,
  title: String,
  message: String,
) -> element.Element(message.Msg) {
  html.div([att.class("error-notification")], [
    html.div([att.class("error-notification-body")], [
      html.h2([att.class("error-notification-title")], [html.text(title)]),
      html.p([att.class("error-notification-message")], [html.text(message)]),
    ]),

    html.button(
      [
        att.class("ghost"),
        event.on_click(message.DismissError(id)),
      ],
      [icon(graphics.icon_close, MediumSize)],
    ),
  ])
}

/// Trigger on_click event and then stop event propagation.
pub fn click_stop(msg) {
  event.on_click(msg) |> event.stop_propagation()
}

/// Trigger on_mouse_down event and then stop event propagation.
pub fn down_stop(msg) {
  event.on_mouse_down(msg) |> event.stop_propagation()
}

/// Trigger on_mouse_down event and then stop event propagation.
pub fn up_stop(msg) {
  event.on_mouse_up(msg) |> event.stop_propagation()
}

/// Trigger on_mouse_over event and then stop event propagation.
pub fn over_stop(msg) {
  event.on_mouse_over(msg) |> event.stop_propagation()
}
