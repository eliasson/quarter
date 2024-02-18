using System;
using System.Collections.Generic;
using System.Linq;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State.ViewModels;

namespace Quarter.State;

public record ModalState(Type ModalType, Dictionary<string, object> Parameters)
{
    public static ModalState ParameterLess(Type modalType)
        => new(modalType, new Dictionary<string, object>());
}

public record SelectedActivity(IdOf<Project> ProjectId, IdOf<Activity> ActivityId);

/// <summary>
/// Global application state that is available to all QuarterComponents.
///
/// - Components must not change state directly, only via dispatching actions.
/// </summary>
public class ApplicationState
{
    public const string FormData = nameof(FormData);
    public const string ModalTitle = nameof(ModalTitle);

    // Keep some potentially configurable items in the application state. These might be user, locale or context aware
    // in the near future.
    public static readonly ActivityColor EraseColor = new ActivityColor("#ffffff", "#ababac");
    public static readonly ActivityColor DefaultActivityColor = new ActivityColor("#ffffff", "#cccccc");
    public const int DefaultStartHour = 6;
    public const int DefaultEndHour = 18;

    public Stack<ModalState> Modals { get; } = new();

    public List<ProjectViewModel> Projects { get; set; } = new();

    public Timesheet? SelectedTimesheet { get; set; }
    public Date? SelectedDate { get; set; }
    public SelectedActivity? SelectedActivity { get; set; }

    /// <summary>
    /// The number of state changes that has been made since the application state was created.
    /// Usable during debugging
    /// </summary>
    public long StateChanges { get; set; }

    public void SafePopTopMostModal()
        => Modals.TryPop(out _);

    public ActivityViewModel? GetSelectedActivity()
    {
        if (SelectedActivity is null) return null;

        var project = Projects.Find(p => p.Id == SelectedActivity.ProjectId);
        var activity = project?.Activities.ToList().Find(a => a.Id == SelectedActivity.ActivityId);

        return activity;
    }
}
