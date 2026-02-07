import domain/color
import gleam/list
import gleeunit/should

pub fn can_parse_hex_string_to_color_test() {
  let tests = [
    #("#FFFFFF", color.Color(255, 255, 255)),
    #("#000000", color.Color(0, 0, 0)),
    #("#8E87F5", color.Color(142, 135, 245)),
    #("8E87F5", color.Color(142, 135, 245)),
  ]

  list.each(tests, fn(t) {
    case color.from_hex(t.0) {
      Ok(actual) -> should.equal(actual, t.1)
      Error(_) -> should.fail()
    }
  })
}

pub fn can_not_parse_invalid_hex_string_to_color_test() {
  let tests = [
    "white",
    "#FFFFFFFF",
    "#FFF",
    "",
  ]

  list.each(tests, fn(t) {
    case color.from_hex(t) {
      Ok(_) -> should.fail()
      Error(message) -> should.equal(message, "Invalid color value")
    }
  })
}

pub fn can_convert_color_to_hex_test() {
  let tests = [
    #(color.Color(255, 255, 255), "#FFFFFF"),
    #(color.Color(0, 0, 0), "#000000"),
    #(color.Color(142, 135, 245), "#8E87F5"),
    #(color.Color(255, 0, 0), "#FF0000"),
    #(color.Color(0, 255, 0), "#00FF00"),
    #(color.Color(0, 0, 255), "#0000FF"),
  ]

  list.each(tests, fn(t) {
    let actual = color.to_hex(t.0)
    should.equal(actual, t.1)
  })
}

pub fn can_convert_single_digit_hex_values_test() {
  let tests = [
    #(color.Color(1, 2, 3), "#010203"),
    #(color.Color(15, 15, 15), "#0F0F0F"),
    #(color.Color(0, 0, 0), "#000000"),
    #(color.Color(16, 32, 48), "#102030"),
  ]

  list.each(tests, fn(t) {
    let actual = color.to_hex(t.0)
    should.equal(actual, t.1)
  })
}

pub fn can_round_trip_hex_conversion_test() {
  let colors = [
    color.Color(255, 255, 255),
    color.Color(0, 0, 0),
    color.Color(142, 135, 245),
    color.Color(1, 2, 3),
    color.Color(128, 64, 192),
  ]

  list.each(colors, fn(original) {
    let hex = color.to_hex(original)
    case color.from_hex(hex) {
      Ok(parsed) -> should.equal(parsed, original)
      Error(_) -> should.fail()
    }
  })
}

pub fn to_hex_returns_uppercase_test() {
  let actual = color.to_hex(color.Color(171, 205, 239))
  should.equal(actual, "#ABCDEF")
}
