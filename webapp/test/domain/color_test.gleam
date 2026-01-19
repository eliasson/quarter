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
