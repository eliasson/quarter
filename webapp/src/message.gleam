import model
import project
import route.{type Route}
import rsvp
import user

pub type Msg {
  Noop
  /// When a page navigation takes place.
  OnRouteChange(Route)

  /// Generic drop down menu that is keyed with an arbitrary key.
  OpenDropDownMenu(id: String)

  /// Opens the given dialog.
  OpenDialog(dialog: model.Dialog)

  ///When the modal backdrop is clicked, menu closed, etc.
  CloseModal

  /// When a dialog is confirmed (save, ok, etc).
  ConfirmDialog

  /// Dismiss an error notification.
  DismissError(id: String)

  /// Select / deselect a project in the mange project list.
  ToggleProject(id: project.ProjectId)

  /// Action that asks the user to confirm archive of the given activity.
  ConfirmArchiveActivity(activity: project.Activity)

  /// Archive the given activity.
  ArchiveActivity(activity: project.Activity)

  //
  // Form messages
  //
  /// A text field was updated
  FormTextFieldUpdated(value: model.FormValue)

  //
  // Protocol messages
  //
  CurrentUserResult(Result(user.User, rsvp.Error))
  SystemUsersResult(Result(List(user.User), rsvp.Error))
  AddUserResult(Result(user.User, rsvp.Error))
  ProjectsResult(Result(List(project.Project), rsvp.Error))
}
