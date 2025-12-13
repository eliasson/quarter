import gleam/list
import gleeunit/should
import util.{Color, Email}

pub fn valid_emails_test() {
  let tests = [
    Email("jane@example.com"),
    Email("jane.doe@example.com"),
    Email("a@b.c"),
    Email("jane@EXAMPLE.com"),
    Email("jane@example.corp.co.uk"),
  ]
  list.each(tests, fn(v) { should.be_ok(util.validate_email(v)) })
}

pub fn invalid_emails_test() {
  let tests = [
    Email("jane@example"),
    Email("janeexample.com"),
    Email("jane @example.com"),
    Email("@example.com"),
  ]
  list.each(tests, fn(v) { should.be_error(util.validate_email(v)) })
}

pub fn invalid_email_should_include_error_message_test() {
  case util.validate_email(Email("jane@example")) {
    Ok(Nil) -> should.fail()
    Error(messages) -> should.equal(messages, ["Invalid e-mail address"])
  }
}

pub fn can_parse_hex_string_to_color_test() {
  let tests = [
    #("#FFFFFF", Color(255, 255, 255)),
    #("#000000", Color(0, 0, 0)),
    #("#8E87F5", Color(142, 135, 245)),
    #("8E87F5", Color(142, 135, 245)),
  ]

  list.each(tests, fn(t) {
    case util.from_hex(t.0) {
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
    case util.from_hex(t) {
      Ok(_) -> should.fail()
      Error(message) -> should.equal(message, "Invalid color value")
    }
  })
}
