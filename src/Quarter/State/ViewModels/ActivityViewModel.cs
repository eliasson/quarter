using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.State.ViewModels;

public class ActivityViewModel
{
    /// <summary>
    /// The unique ID of this activity
    /// </summary>
    public IdOf<Activity> Id { get; set; } = IdOf<Activity>.None;

    /// <summary>
    /// The ID of the project this activity is associated with
    /// </summary>
    public IdOf<Project> ProjectId { get; set; } = IdOf<Project>.None;

    /// <summary>
    /// The name of the activity
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Describes the activity
    /// </summary>
    public string Description { get; set; } = "";

    public string Color { get; set; } = "";

    public string DarkerColor { get; set; } = "";

    /// <summary>
    /// The last time this activity was updated (or created if never updated)
    /// </summary>
    public UtcDateTime? Updated { get; set; }

    /// <summary>
    /// The last time this activity was used to regiester time
    /// </summary>
    public UtcDateTime? LastUsed { get; set; }

    /// <summary>
    /// The total usage in minutes for this activity
    /// </summary>
    public int TotalMinutes { get; set; }

    /// <summary>
    /// Return the total usage in hours represented by a string formatted as (hh:hh) e.g. 2.50
    /// </summary>
    /// <returns></returns>
    public string TotalAsHours()
        => ((float) TotalMinutes / 60.0).ToString("F2");
}