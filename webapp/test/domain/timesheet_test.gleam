import domain/duration
import gleeunit/should
import test_util

pub fn it_should_be_empty_test() {
  let sheet =
    test_util.new_timesheet("2026-02-07T18:00:00Z")
    |> should.be_ok

  sheet.slots |> should.equal([])
  sheet.duration |> should.equal(duration.Minutes(0))
}
