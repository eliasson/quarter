import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div, h1, p}
import message
import model
import util/timestamp

pub fn view(m: model.Model) -> Element(message.Msg) {
  let month = i18n.name_of_month(m.today, m.lang) |> i18n.capitalize
  let date = timestamp.to_iso_date(m.today)

  div([att.class("content")], [
    div([att.class("content-heading")], [
      div([], [
        h1([], [html.text(month)]),
        p([], [
          html.text("Today is " <> date),
        ]),
      ]),
    ]),
  ])
}
