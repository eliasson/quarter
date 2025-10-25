import lustre/attribute as att
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
