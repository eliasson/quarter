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
  let bb = float.add(b_float, float.multiply(b_float, lum))

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

/// Returns a dark variant of the color for bright backgrounds, or a bright
/// variant for dark backgrounds, preserving hue and saturation.
/// Uses YIQ luminance to decide, then adjusts HSL lightness to 15% or 85%.
pub fn text_color(bg: Color) -> Color {
  let luminance =
    {
      0.299
      *. int.to_float(bg.r)
      +. 0.587
      *. int.to_float(bg.g)
      +. 0.114
      *. int.to_float(bg.b)
    }
    /. 255.0
  case luminance >=. 0.5 {
    True -> set_lightness(bg, 0.15)
    False -> set_lightness(bg, 0.8)
  }
}

fn set_lightness(color: Color, target_l: Float) -> Color {
  let r = int.to_float(color.r) /. 255.0
  let g = int.to_float(color.g) /. 255.0
  let b = int.to_float(color.b) /. 255.0

  let cmax = float.max(float.max(r, g), b)
  let cmin = float.min(float.min(r, g), b)

  case cmax == cmin {
    True -> {
      let v = float.round(target_l *. 255.0)
      Color(v, v, v)
    }
    False -> {
      let d = cmax -. cmin
      let l = { cmax +. cmin } /. 2.0
      let s = case l >. 0.5 {
        True -> d /. { 2.0 -. cmax -. cmin }
        False -> d /. { cmax +. cmin }
      }
      let h = case cmax == r, cmax == g {
        True, _ -> {
          let base = { g -. b } /. d
          case g <. b {
            True -> { base +. 6.0 } /. 6.0
            False -> base /. 6.0
          }
        }
        False, True -> { { b -. r } /. d +. 2.0 } /. 6.0
        False, False -> { { r -. g } /. d +. 4.0 } /. 6.0
      }

      let q = case target_l <. 0.5 {
        True -> target_l *. { 1.0 +. s }
        False -> target_l +. s -. target_l *. s
      }
      let p = 2.0 *. target_l -. q

      Color(
        float.round(hue_to_rgb(p, q, h +. 1.0 /. 3.0) *. 255.0),
        float.round(hue_to_rgb(p, q, h) *. 255.0),
        float.round(hue_to_rgb(p, q, h -. 1.0 /. 3.0) *. 255.0),
      )
    }
  }
}

fn hue_to_rgb(p: Float, q: Float, t: Float) -> Float {
  let t = case t <. 0.0 {
    True -> t +. 1.0
    False -> t
  }
  let t = case t >. 1.0 {
    True -> t -. 1.0
    False -> t
  }
  case t <. 1.0 /. 6.0 {
    True -> p +. { q -. p } *. 6.0 *. t
    False ->
      case t <. 0.5 {
        True -> q
        False ->
          case t <. 2.0 /. 3.0 {
            True -> p +. { q -. p } *. { 2.0 /. 3.0 -. t } *. 6.0
            False -> p
          }
      }
  }
}

fn zero_pad(s: String) -> String {
  case string.length(s) {
    1 -> "0" <> s
    _ -> s
  }
}
