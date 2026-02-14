import i18n
import lustre/attribute as att
import lustre/element.{type Element}
import lustre/element/html.{div}
import message
import model

pub fn view(m: model.Model) -> Element(message.Msg) {
  let date = i18n.date_long(m.today, m.lang) |> i18n.capitalize()

  div([att.class("content")], [div([], [html.text(date)])])
}
