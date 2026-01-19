import gleam/float
import gleam/int
import gleam/regexp
import gleam/string
import gleam/time/timestamp

pub type Email {
  Email(value: String)
}

pub type Either(a, b) {
  Left(a)
  Right(b)
}

pub fn timestamp_zero() {
  timestamp.from_unix_seconds(0)
}

pub fn validate_email(email: Email) -> Result(Nil, List(String)) {
  // Simple validation. There should be something an @ and then something with a .
  let email_pattern = "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$"
  case regexp.from_string(email_pattern) {
    Ok(re) -> {
      case regexp.check(with: re, content: email.value) {
        True -> Ok(Nil)
        False -> Error(["Invalid e-mail address"])
      }
    }
    // If there is an error, then most likely our regexp does not compile.
    Error(_) -> Error(["Invalid e-mail address"])
  }
}
