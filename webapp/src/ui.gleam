import gleam/option.{type Option}
import lustre/attribute as att
import lustre/element.{type Element}
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

pub fn drop_down_item(url: String, icon ico: String, text text: String) {
  drop_down_item_impl(url, ico, text, option.None)
}

pub fn drop_down_item_extended(
  url: String,
  icon ico: String,
  text text: String,
  appendix appendix: String,
) {
  drop_down_item_impl(url, ico, text, option.Some(appendix))
}

pub fn drop_down_item_impl(
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

pub fn separator_menu_item() {
  html.hr([att.class("separator")])
}
