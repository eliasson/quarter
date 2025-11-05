import gfx
import gleam/int
import gleam/option.{type Option}
import gleam/time/calendar
import gleam/time/timestamp
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/element/svg
import lustre/event

pub fn icon(icon_name: String) {
  // I did not manage to produce an attribute using xlink:href, but only href
  // seems to work fine in Firefox at least.
  //
  // NOTE lustre/eleemnt have namespaced function, try that!
  html.svg([att.class("icon")], [
    svg.use_([att.attribute("href", "#" <> icon_name)]),
  ])
}

pub fn drop_down_item(
  url: String,
  icon ico: String,
  text text: String,
) -> element.Element(a) {
  drop_down_item_impl(url, ico, text, option.None)
}

pub fn drop_down_item_extended(
  url: String,
  icon ico: String,
  text text: String,
  appendix appendix: String,
) -> element.Element(a) {
  drop_down_item_impl(url, ico, text, option.Some(appendix))
}

pub fn drop_down_header(on_close: msg) -> element.Element(msg) {
  html.div([att.class("drop-down-menu-header")], [
    html.div([att.class("content")], [
      icon(gfx.icon_logo),
      close_button(on_close),
    ]),
  ])
}

pub fn separator_menu_item() {
  html.hr([att.class("separator")])
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

fn drop_down_item_impl(
  url: String,
  icon ico: String,
  text text: String,
  appendix appendix: Option(String),
) {
  let appendix_elm = case appendix {
    option.Some(t) -> html.div([att.class("appendix")], [html.text(t)])
    option.None -> element.none()
  }

  html.div([att.class("drop-down-menu-item")], [
    html.div([att.class("content")], [
      icon(ico),
      html.a([att.href(url)], [html.text(text)]),
    ]),
    appendix_elm,
  ])
}

fn close_button(on_click: msg) -> element.Element(msg) {
  html.button(
    [
      att.class("ghost"),
      event.on_click(on_click),
    ],
    [
      icon(gfx.icon_close),
    ],
  )
}
