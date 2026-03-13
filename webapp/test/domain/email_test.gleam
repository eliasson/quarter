import domain/email
import gleam/list
import gleeunit/should

pub fn valid_emails_test() {
  let tests = [
    email.Email("jane@example.com"),
    email.Email("jane.doe@example.com"),
    email.Email("a@b.c"),
    email.Email("jane@EXAMPLE.com"),
    email.Email("jane@example.corp.co.uk"),
  ]
  list.each(tests, fn(v) { should.be_ok(email.validate_email(v)) })
}

pub fn invalid_emails_test() {
  let tests = [
    email.Email("jane@example"),
    email.Email("janeexample.com"),
    email.Email("jane @example.com"),
    email.Email("@example.com"),
  ]
  list.each(tests, fn(v) { should.be_error(email.validate_email(v)) })
}

pub fn invalid_email_should_include_error_message_test() {
  case email.validate_email(email.Email("jane@example")) {
    Ok(Nil) -> should.fail()
    Error(messages) -> should.equal(messages, ["Invalid e-mail address"])
  }
}
