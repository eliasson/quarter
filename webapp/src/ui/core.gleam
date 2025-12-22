import gleam/int
import gleam/time/calendar
import gleam/time/timestamp
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/element/svg
import lustre/event

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
  // This is just the simplest thing possible.
  // Find a third-party library that makes this nicer, supporting user timezone, etc.
  // Or implement a ffi for JavaScript Intl.
  let c = timestamp.to_calendar(ts, calendar.utc_offset)

  let value =
    ""
    <> int.to_string({ c.0 }.year)
    <> "-"
    <> int.to_string(calendar.month_to_int({ c.0 }.month))
    <> "-"
    <> int.to_string({ c.0 }.day)
    <> " "
    <> int.to_string({ { c.1 }.hours })
    <> ":"
    <> int.to_string({ { c.1 }.minutes })
    <> ":"
    <> int.to_string({ { c.1 }.seconds })
  html.time([], [html.text(value)])
}

pub fn toolbar(children: List(element.Element(msg))) {
  html.div([att.class("toolbar")], children)
}

/// Trigger on_click event and then stop event propagation.
pub fn click_stop(msg) {
  event.on_click(msg) |> event.stop_propagation()
}

pub fn dialog(ico: String, header: String) {
  html.div([att.class("dialog")], [
    html.div([att.class("dialog-backdrop")], []),
    html.div([att.class("dialog-overlay")], [
      html.div([att.class("dialog-container")], [
        html.div([att.class("dialog-header")], [
          icon(ico, MediumSize),
          html.span([], [html.text(header)]),
        ]),
        html.div([att.class("dialog-content")], [
          // Dialog content
        ]),
        html.div([att.class("dialog-footer")], [
          // Footer actions
        ]),
      ]),
    ]),
  ])
}
