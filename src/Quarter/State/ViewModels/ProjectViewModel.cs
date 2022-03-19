using System.Collections.Generic;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.State.ViewModels;

public class ProjectViewModel
{
    public IdOf<Project> Id { get; set; } = IdOf<Project>.None;

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    /// <summary>
    /// The last time this project was updated (or created if never updated)
    /// </summary>
    public UtcDateTime? Updated { get; set; }

    /// <summary>
    /// The last time an activity for this project was used
    /// </summary>
    public UtcDateTime? LastUsed { get; set; }

    /// <summary>
    /// The total usage in minutes for all activities (regardless of state) for this project
    /// </summary>
    public int TotalMinutes { get; set; }

    /// <summary>
    /// All activities associated with this project
    /// </summary>
    public IList<ActivityViewModel> Activities { get; set; } = new List<ActivityViewModel>();
}
