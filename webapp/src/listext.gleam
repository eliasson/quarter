import gleam/list

/// Drop the last element from the given list.
pub fn drop_last(lst: List(a)) -> List(a) {
  case list.length(lst) {
    0 -> []
    n -> list.take(lst, n - 1)
  }
}
