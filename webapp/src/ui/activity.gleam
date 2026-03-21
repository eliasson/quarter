import domain/color
import domain/project
import lustre/attribute as att
import lustre/element/html.{div}
import ui/core as ui
import ui/markup

/// Render the common round color badge for an activity.
pub fn activity_badge(activity: project.Activity, size: ui.Size) {
  activitiy_color_badge(activity.color, size)
}

/// Render the common round color badge for an activity.
pub fn activitiy_color_badge(c: color.Color, size: ui.Size) {
  let border_color = color.darken(c)

  let classes =
    [att.class("activity-color")]
    |> markup.cond_class(size == ui.SmallSize, "small")

  div(
    [
      att.styles([
        #("background-color", color.color_to_style_value(c)),
        #("border-color", color.color_to_style_value(border_color)),
      ]),
      ..classes
    ],
    [],
  )
}
