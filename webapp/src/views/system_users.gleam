import gleam/int
import gleam/list
import gleam/time/calendar
import gleam/time/timestamp

import lustre/element.{type Element}
import lustre/element/html.{div}
import message
import model

pub fn view(m: model.Model) -> Element(message.Msg) {
  div([], list_users(m))
}

fn list_users(m: model.Model) {
  list.map(m.users, fn(u) {
    div([], [html.text(u.email), html.text(timestamp_to_string(u.created))])
  })
}

fn timestamp_to_string(ts: timestamp.Timestamp) -> String {
  let c = timestamp.to_calendar(ts, calendar.utc_offset)

  ""
  <> int.to_string({ c.0 }.year)
  <> int.to_string(calendar.month_to_int({ c.0 }.month))
  <> int.to_string({ c.0 }.day)
}
