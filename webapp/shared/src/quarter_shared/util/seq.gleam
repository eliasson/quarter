import gleam/dict.{type Dict}
import gleam/list
import gleam/option

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

/// Add a new list with the given value for the given key, or if the
/// key exists, append the value to the existing list.
pub fn add_or_append(
  d: dict.Dict(a, List(b)),
  key: a,
  value: b,
) -> dict.Dict(a, List(b)) {
  let v = case dict.get(d, key) {
    Ok(lst) -> list.append(lst, [value])
    Error(_) -> [value]
  }
  dict.insert(d, key, v)
}

/// Groups a list of items by a key derived from each item.
pub fn group_by(items: List(v), key: fn(v) -> k) -> Dict(k, List(v)) {
  list.fold(items, dict.new(), fn(acc, item) {
    dict.upsert(acc, key(item), fn(existing) {
      case existing {
        option.Some(group) -> [item, ..group]
        option.None -> [item]
      }
    })
  })
}
