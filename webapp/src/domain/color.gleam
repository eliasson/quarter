import gleam/float
import gleam/int
import gleam/string

/// Represents a RGBA color
/// The backend does not use HEX colors with  alpha currently.
pub type Color {
  // TODO Store color as float
  Color(r: Int, g: Int, b: Int)
}

pub fn darken(color: Color) -> Color {
  // Math.round(Math.min(Math.max(0, c + (c * lum)), 255))
  let lum = -0.3
  let r_float = int.to_float(color.r)
  let g_float = int.to_float(color.g)
  let b_float = int.to_float(color.b)

  let rr = float.add(r_float, float.multiply(r_float, lum))
  let gg = float.add(g_float, float.multiply(g_float, lum))
  let bb = float.add(b_float, float.multiply(g_float, lum))

  let r = float.round(float.min(float.max(0.0, rr), 255.0))
  let g = float.round(float.min(float.max(0.0, gg), 255.0))
  let b = float.round(float.min(float.max(0.0, bb), 255.0))

  Color(r, g, b)
}

pub fn color_to_style_value(c: Color) -> String {
  "rgb("
  <> int.to_string(c.r)
  <> ", "
  <> int.to_string(c.g)
  <> ", "
  <> int.to_string(c.b)
  <> ")"
}

/// Converts a color to a hexadecimal string.
pub fn to_hex(c: Color) -> String {
  let r = zero_pad(int.to_base16(c.r))
  let g = zero_pad(int.to_base16(c.g))
  let b = zero_pad(int.to_base16(c.b))

  "#" <> r <> g <> b
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

fn zero_pad(s: String) -> String {
  case string.length(s) {
    1 -> "0" <> s
    _ -> s
  }
}
