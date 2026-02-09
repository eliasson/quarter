import gleam/time/calendar
import gleam/time/timestamp
import gleeunit/should
import i18n

pub fn should_describe_translation_test() {
  i18n.Translation("hello")
  |> i18n.describe
  |> should.equal("hello")
}

pub fn should_capitalize_translation_test() {
  i18n.Translation("hello")
  |> i18n.capitalize
  |> should.equal("Hello")
}

pub fn should_translate_month_test() {
  let t =
    timestamp.parse_rfc3339("2022-01-01T00:00:00Z")
    |> should.be_ok
    |> i18n.name_of_month(i18n.English)

  should.equal(t.value, "january")
}

pub fn should_translate_year_test() {
  let t =
    timestamp.parse_rfc3339("2022-01-01T00:00:00Z")
    |> should.be_ok
    |> i18n.year(i18n.English)

  should.equal(t.value, "2022")
}

pub fn should_translate_day_test() {
  let t =
    timestamp.parse_rfc3339("2022-02-04T00:00:00Z")
    |> should.be_ok
    |> i18n.day(i18n.English)

  should.equal(t.value, "4")
}
