import gfx
import gleam/int
import gleam/list
import gleam/option.{type Option}
import gleam/time/calendar
import gleam/time/timestamp
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/element/svg
import lustre/event
import message
import util

pub type Size {
  SmallSize
  MediumSize
}

pub type DropDownItem {
  DropDownLink(icon: String, label: String, href: String)
  DropDownMsg(icon: String, label: String, msg: message.Msg)
  DropDownLinkApx(
    icon: String,
    label: String,
    description: String,
    href: String,
  )
  DropDownHeader
  DropDownSeparator
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

pub fn drop_down_menu(
  id: String,
  initiating: element.Element(message.Msg),
  menu_items: List(DropDownItem),
  is_open: Bool,
) -> element.Element(message.Msg) {
  let menu = case is_open {
    True ->
      html.div([att.class("drop-down-menu")], [
        drop_down_header(message.CloseModal),
        ..list.map(menu_items, fn(i) { create_drop_down_item(i) })
      ])
    _ -> element.none()
  }

  html.div(
    [
      att.class("drop-down-initiator"),
      event.on_click(message.OpenDropDownMenu(id)),
    ],
    [initiating, menu],
  )
}

fn create_drop_down_item(item: DropDownItem) -> element.Element(message.Msg) {
  case item {
    DropDownLink(icon, label, url) ->
      drop_down_item_impl(util.Left(url), icon, label, option.None)

    DropDownLinkApx(icon, label, appendix, url) ->
      drop_down_item_impl(util.Left(url), icon, label, option.Some(appendix))

    DropDownMsg(icon, label, msg) ->
      drop_down_item_impl(util.Right(msg), icon, label, option.None)

    DropDownSeparator -> separator_menu_item()

    DropDownHeader -> drop_down_header(message.CloseModal)
  }
}

fn drop_down_header(on_close: msg) -> element.Element(msg) {
  html.div([att.class("drop-down-menu-header")], [
    html.div([att.class("content")], [
      icon(gfx.icon_logo, MediumSize),
      close_button(on_close),
    ]),
  ])
}

fn separator_menu_item() {
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

pub fn checkbox() -> element.Element(msg) {
  html.label([att.class("checkbox")], [html.input([att.type_("checkbox")])])
}

pub fn ghost_button(icon ico: String) {
  html.button([att.class("ghost")], [icon(ico, MediumSize)])
}

pub fn outline_button(text: String, icon ico: String) {
  html.button([att.class("ghost")], [html.text(text), icon(ico, SmallSize)])
}

pub fn toolbar(children: List(element.Element(msg))) {
  html.div([att.class("toolbar")], children)
}

fn drop_down_item_impl(
  action: util.Either(String, message.Msg),
  icon ico: String,
  text text: String,
  appendix appendix: Option(String),
) {
  let appendix_elm = case appendix {
    option.Some(t) -> html.div([att.class("appendix")], [html.text(t)])
    option.None -> element.none()
  }

  // TODO: Move the A to wrap the entire item to make it clickable.
  // TODO: Should a BUTTON be used instead of a DIV to work with keyboard navigation.
  let label_elm = case action {
    util.Left(url) -> html.a([att.href(url)], [html.text(text)])
    util.Right(msg) ->
      html.div([click_stop(msg)], [
        html.text(text),
      ])
  }

  html.div([att.class("drop-down-menu-item")], [
    html.div([att.class("content")], [icon(ico, MediumSize), label_elm]),
    appendix_elm,
  ])
}

fn close_button(on_click: msg) -> element.Element(msg) {
  html.button(
    [
      att.class("ghost"),
      click_stop(on_click),
    ],
    [
      icon(gfx.icon_close, MediumSize),
    ],
  )
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

/// Trigger on_click event and then stop event propagation.
pub fn click_stop(msg) {
  event.on_click(msg) |> event.stop_propagation()
}
