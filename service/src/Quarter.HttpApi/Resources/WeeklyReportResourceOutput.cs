using Quarter.Core.Queries;

// ReSharper disable InconsistentNaming

namespace Quarter.HttpApi.Resources;

public record ProjectWeekUsageResource(string projectId, int totalMinutes, IList<ActivityWeekUsageResource> activityUsage)
{
    public static ProjectWeekUsageResource From(ProjectWeekUsage usage)
    {
        var activityUsage = usage.Usage.Values.Select(ActivityWeekUsageResource.From).ToList();
        return new ProjectWeekUsageResource(usage.ProjectId.AsString(), usage.TotalMinutes, activityUsage);
    }
}

public record ActivityWeekUsageResource(string activityId, int totalMinutes, int[] weekdayTotals)
{
    public static ActivityWeekUsageResource From(ActivityWeekUsage usage)
    {
        return new ActivityWeekUsageResource(usage.ActivityId.AsString(), usage.TotalMinutes, usage.DurationPerWeekDay);
    }
}

public record WeeklyReportResourceOutput(string startOfWeek, string endOfWeek, int totalMinutes, int[] weekdayTotals, IList<ProjectWeekUsageResource> Usage)
{
    public static WeeklyReportResourceOutput From(WeeklyReportResult result)
    {
        var usage = result.Usage.Values.Select(ProjectWeekUsageResource.From).ToList();

        return new WeeklyReportResourceOutput(
            result.StartOfWeek.IsoString(),
            result.EndOfWeek.IsoString(),
            result.TotalMinutes,
            result.WeekdayTotals,
            usage);
    }
}
