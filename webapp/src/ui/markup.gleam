import gleam/list
import lustre/attribute as att

/// Add conditonal classes to the list of attributes.
pub fn cond_class(classes: List(att.Attribute(a)), cond: Bool, class: String) {
  case cond {
    True -> list.append(classes, [att.class(class)])
    False -> classes
  }
}
