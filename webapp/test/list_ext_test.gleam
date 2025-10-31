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
