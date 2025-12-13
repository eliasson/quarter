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

/// Represents a RGBA color
/// The backend does not use HEX colors with  alpha currently.
pub type Color {
  Color(r: Int, g: Int, b: Int)
}

pub fn from_hex(value: String) -> Result(Color, String) {
  let value = case string.pop_grapheme(value) {
    Ok(#("#", rest)) -> rest
    _ -> value
  }

  case string.length(value) {
    6 -> {
      case
        int.base_parse(string.slice(value, 0, 2), 16),
        int.base_parse(string.slice(value, 2, 2), 16),
        int.base_parse(string.slice(value, 4, 2), 16)
      {
        Ok(r), Ok(g), Ok(b) -> Ok(Color(r, g, b))
        _, _, _ -> Error("Invalid color value")
      }
    }
    _ -> Error("Invalid color value")
  }
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
