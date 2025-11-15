import lustre/attribute as att
import lustre/element
import lustre/element/html
import ui/core as ui

pub fn checkbox() -> element.Element(msg) {
  html.label([att.class("checkbox")], [html.input([att.type_("checkbox")])])
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
