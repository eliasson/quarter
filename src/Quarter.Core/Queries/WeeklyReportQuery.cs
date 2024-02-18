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

public class WeeklyReportResult(Date startOfWeek, Date endOfWeek)
{
    public Date StartOfWeek { get; private set; } = startOfWeek;
    public Date EndOfWeek { get; private set; } = endOfWeek;
    public IDictionary<IdOf<Project>, ProjectWeekUsage> Usage { get; private set; } = new Dictionary<IdOf<Project>, ProjectWeekUsage>();
    public int TotalMinutes { get; private set; }
    public int[] WeekdayTotals { get; private set; } = new int[7];

    public void AddOrUpdate(ProjectSummary projectSummary, int weekDayIndex)
    {
        if (!Usage.TryGetValue(projectSummary.ProjectId, out var projectUsage))
        {
            projectUsage = new ProjectWeekUsage(projectSummary.ProjectId);
            Usage.Add(projectSummary.ProjectId, projectUsage);
        }

        foreach (var activitySummary in projectSummary.Activities)
        {
            projectUsage.AddOrUpdate(activitySummary, weekDayIndex);
        }

        WeekdayTotals[weekDayIndex] += projectUsage.WeekdayTotals[weekDayIndex];
        TotalMinutes += projectSummary.Duration * 15;
    }
}

public class ProjectWeekUsage(IdOf<Project> projectId)
{
    public IdOf<Project> ProjectId { get; private set; } = projectId;
    public readonly IDictionary<IdOf<Activity>, ActivityWeekUsage> Usage = new Dictionary<IdOf<Activity>, ActivityWeekUsage>();
    public int TotalMinutes;
    public int[] WeekdayTotals { get; private set; } = new int[7];

    public void AddOrUpdate(ActivitySummary activitySummary, int weekDayIndex)
    {
        if (!Usage.TryGetValue(activitySummary.ActivityId, out var activityUsage))
        {
            activityUsage = new ActivityWeekUsage(activitySummary.ActivityId);
            Usage.Add(activitySummary.ActivityId, activityUsage);
        }

        activityUsage.AddOrUpdate(activitySummary, weekDayIndex);

        var durationForActivity = activitySummary.Duration * 15;
        TotalMinutes += durationForActivity;
        WeekdayTotals[weekDayIndex] += durationForActivity;
    }
}

public class ActivityWeekUsage(IdOf<Activity> activityId)
{
    public IdOf<Activity> ActivityId { get; private set; } = activityId;
    public readonly int[] DurationPerWeekDay = new int[7];
    public int TotalMinutes;

    public void AddOrUpdate(ActivitySummary activitySummary, int weekDayIndex)
    {
        DurationPerWeekDay[weekDayIndex] += activitySummary.Duration * 15;
        TotalMinutes += activitySummary.Duration * 15;
    }
}
