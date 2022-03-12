using System.Collections.Generic;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Queries;

public class WeeklyReportQuery
{
    public Date From { get; private set; }
    public Date To { get; private set; }

    public WeeklyReportQuery(Date date)
    {
        From = date.StartOfWeek();
        To = date.EndOfWeek();
    }
}

public class WeeklyReportResult
{
    public Date StartOfWeek { get; private set; }
    public Date EndOfWeek { get; private set; }
    public IDictionary<IdOf<Project>, ProjectWeekUsage> Usage { get; private set; }
    public int TotalMinutes { get; private set; }

    public WeeklyReportResult(Date startOfWeek, Date endOfWeek)
    {
        StartOfWeek = startOfWeek;
        EndOfWeek = endOfWeek;
        Usage = new Dictionary<IdOf<Project>, ProjectWeekUsage>();
    }

    /// <summary>
    /// Return the total usage in hours represented by a string formatted as (hh:hh) e.g. 2.50
    /// </summary>
    public string TotalAsHours()
        => ((float) TotalMinutes / 60.0).ToString("F2");

    public void AddOrUpdate(ProjectSummary projectSummary)
    {
        if (!Usage.TryGetValue(projectSummary.ProjectId, out var projectUsage))
        {
            projectUsage = new ProjectWeekUsage(projectSummary.ProjectId);
            Usage.Add(projectSummary.ProjectId, projectUsage);
        }

        foreach (var activitySummary in projectSummary.Activities)
            projectUsage.AddOrUpdate(activitySummary);

        TotalMinutes += projectSummary.Duration * 15;
    }
}

public class ProjectWeekUsage
{
    public IdOf<Project> ProjectId { get; private set; }
    public readonly IDictionary<IdOf<Activity>, ActivityWeekUsage> Usage = new Dictionary<IdOf<Activity>, ActivityWeekUsage>();
    public int TotalMinutes;

    public ProjectWeekUsage(IdOf<Project> projectId)
    {
        ProjectId = projectId;
    }


    public void AddOrUpdate(ActivitySummary activitySummary)
    {
        if (!Usage.TryGetValue(activitySummary.ActivityId, out var activityUsage))
        {
            activityUsage = new ActivityWeekUsage(activitySummary.ActivityId);
            Usage.Add(activitySummary.ActivityId, activityUsage);
        }

        activityUsage.AddOrUpdate(activitySummary);
        TotalMinutes += activitySummary.Duration * 15;
    }
}

public class ActivityWeekUsage
{
    public IdOf<Activity> ActivityId { get; private set; }
    public int[] DurationPerWeekDay; // Always size 7
    public int TotalMinutes;

    public ActivityWeekUsage(IdOf<Activity> activityId)
    {
        ActivityId = activityId;
        DurationPerWeekDay = new int[7];
    }

    public void AddOrUpdate(ActivitySummary activitySummary)
    {
        TotalMinutes += activitySummary.Duration * 15;
    }
}