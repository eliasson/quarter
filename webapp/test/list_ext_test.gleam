import gleam/dict
import gleeunit/should
import listext

pub fn drop_last_test() {
  listext.drop_last([1, 2, 3])
  |> should.equal([1, 2])
}

pub fn drop_last_when_empty_test() {
  listext.drop_last([])
  |> should.equal([])
}

pub fn get_or_empty_returns_empty_when_missing_test() {
  dict.new()
  |> listext.get_or_empty("")
  |> should.equal([])
}

pub fn get_or_empty_returns_value_test() {
  dict.new()
  |> dict.insert("foo", ["a", "b"])
  |> listext.get_or_empty("foo")
  |> should.equal(["a", "b"])
}
