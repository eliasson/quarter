import domain/color
import domain/project
import lustre/attribute as att
import lustre/element/html.{div}

/// Render the common round color badge for an activity.
pub fn activity_badge(activity: project.Activity) {
  activitiy_color_badge(activity.color)
}

/// Render the common round color badge for an activity.
pub fn activitiy_color_badge(c: color.Color) {
  let border_color = color.darken(c)

  div(
    [
      att.class("activity-color"),
      att.styles([
        #("background-color", color.color_to_style_value(c)),
        #("border-color", color.color_to_style_value(border_color)),
      ]),
    ],
    [],
  )
}
