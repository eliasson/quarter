import gleeunit/should
import model
import test_util.{arbitrary_activity}

pub fn close_all_modals_test() {
  let actual =
    model.initial_model()
    |> model.open_drop_down_menu("Test")
    |> model.open_dialog(model.ArchiveActivityDialog(arbitrary_activity()))
    |> model.close_all_modals()

  should.equal(actual.dialogs, [])
  should.equal(actual.dropdowns, [])
}
