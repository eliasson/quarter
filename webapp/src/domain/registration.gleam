import domain/project
import gleam/int
import gleam/option

import types.{type QuarterIndex}

pub type ActiveRegistration {
  /// The ongoing registration
  /// - start - The quarter index where the user first started the registration.
  /// - end - The current quarter index where the registration should end. This can either be before or after the start!
  /// - activity - The activity to register time on, or None to erase time.
  ActiveRegistration(
    start: QuarterIndex,
    end: QuarterIndex,
    activity: option.Option(project.Activity),
  )
}

pub fn is_quarter_active_selection(
  reg: ActiveRegistration,
  index: QuarterIndex,
) -> Bool {
  let start = int.min(reg.start, reg.end)
  let end = int.max(reg.start, reg.end)

  index >= start && index <= end
}
