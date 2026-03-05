import domain/color
import domain/project
import lustre/attribute as att
import lustre/element/html.{div}

/// Render the common round color badge for an activity.
pub fn activity_color_badge(activity: project.Activity) {
  let border_color = color.darken(activity.color)

  div(
    [
      att.class("activity-color"),
      att.styles([
        #("background-color", color.color_to_style_value(activity.color)),
        #("border-color", color.color_to_style_value(border_color)),
      ]),
    ],
    [],
  )
}
