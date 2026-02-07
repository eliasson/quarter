import gleeunit/should
import protocol

pub fn timesheet_decoder_exists_test() {
  // Just verify the decoder function exists and returns a decoder
  let _decoder = protocol.timesheet_decoder()
  Nil |> should.equal(Nil)
}

pub fn timesheets_response_decoder_exists_test() {
  // Just verify the decoder function exists and returns a decoder
  let _decoder = protocol.timesheets_response_decoder()
  Nil |> should.equal(Nil)
}
