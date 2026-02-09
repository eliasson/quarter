import domain/duration
import gleam/list
import gleeunit/should

pub fn various_durations_test() {
  let tests = [
    #(0, #(0, 0)),
    #(1, #(0, 1)),
    #(45, #(0, 45)),
    #(59, #(0, 59)),
    #(60, #(1, 0)),
    #(90, #(1, 30)),
    #(120, #(2, 0)),
    #(480, #(8, 0)),
    #(1440, #(24, 0)),
  ]

  list.each(tests, fn(test_case) {
    let #(minutes, expected) = test_case
    let actual = duration.to_hours_and_minutes(duration.Minutes(minutes))
    should.equal(actual, expected)
  })
}

pub fn negative_duration_test() {
  duration.Minutes(-90)
  |> duration.to_hours_and_minutes
  |> should.equal(#(-1, -30))
}
