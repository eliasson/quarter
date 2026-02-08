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
