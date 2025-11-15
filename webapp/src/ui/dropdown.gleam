import gfx
import gleam/list
import gleam/option.{type Option}
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/event
import message
import ui/core as ui
import util

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
      ui.icon(gfx.icon_logo, ui.MediumSize),
      close_button(on_close),
    ]),
  ])
}

fn separator_menu_item() {
  html.hr([att.class("separator")])
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
      html.div([ui.click_stop(msg)], [
        html.text(text),
      ])
  }

  html.div([att.class("drop-down-menu-item")], [
    html.div([att.class("content")], [ui.icon(ico, ui.MediumSize), label_elm]),
    appendix_elm,
  ])
}

fn close_button(on_click: msg) -> element.Element(msg) {
  html.button(
    [
      att.class("ghost"),
      ui.click_stop(on_click),
    ],
    [
      ui.icon(gfx.icon_close, ui.MediumSize),
    ],
  )
}
