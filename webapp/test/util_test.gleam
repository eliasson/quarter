import gleam/list
import gleeunit/should
import util.{Email}

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
