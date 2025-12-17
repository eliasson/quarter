import gleam/dict
import gleam/list

/// Drop the last element from the given list.
pub fn drop_last(lst: List(a)) -> List(a) {
  case list.length(lst) {
    0 -> []
    n -> list.take(lst, n - 1)
  }
}

/// Get the value by key or an empty list.
pub fn get_or_empty(d: dict.Dict(a, List(b)), key: a) -> List(b) {
  case dict.get(d, key) {
    Ok(r) -> r
    _ -> []
  }
}
