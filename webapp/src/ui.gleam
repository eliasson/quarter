import gfx
import gleam/option.{type Option}
import lustre/attribute as att
import lustre/element
import lustre/element/html
import lustre/element/svg

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

pub fn drop_down_header() -> element.Element(a) {
  html.div([att.class("drop-down-menu-header")], [
    html.div([att.class("content")], [icon(gfx.icon_logo), close_button()]),
  ])
}

pub fn separator_menu_item() {
  html.hr([att.class("separator")])
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

fn close_button() -> element.Element(a) {
  html.button([att.class("ghost")], [
    icon(gfx.icon_close),
  ])
}
