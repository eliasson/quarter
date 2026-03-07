import gleam/dict
import gleeunit/should
import util/seq

pub fn drop_last_test() {
  seq.drop_last([1, 2, 3])
  |> should.equal([1, 2])
}

pub fn drop_last_when_empty_test() {
  seq.drop_last([])
  |> should.equal([])
}

pub fn get_or_empty_returns_empty_when_missing_test() {
  dict.new()
  |> seq.get_or_empty("")
  |> should.equal([])
}

pub fn get_or_empty_returns_value_test() {
  dict.new()
  |> dict.insert("foo", ["a", "b"])
  |> seq.get_or_empty("foo")
  |> should.equal(["a", "b"])
}

pub fn add_to_dict_value_test() {
  dict.new()
  |> seq.add_or_append("foo", "b")
  |> dict.get("foo")
  |> should.be_ok
  |> should.equal(["b"])
}

pub fn append_to_dict_value_test() {
  dict.new()
  |> dict.insert("foo", ["a"])
  |> seq.add_or_append("foo", "b")
  |> dict.get("foo")
  |> should.be_ok
  |> should.equal(["a", "b"])
}
