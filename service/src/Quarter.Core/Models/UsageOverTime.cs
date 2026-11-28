using System;
using System.Collections.Generic;
using System.Linq;
using Quarter.Core.Utils;

namespace Quarter.Core.Models;


public record ActivityUsage(IdOf<Activity> ActivityId, int TotalMinutes, UtcDateTime LastUsed);

public record ProjectTotalUsage(IdOf<Project> ProjectId, int TotalMinutes, List<ActivityUsage> Activities, UtcDateTime LastUsed)
{
    public static readonly ProjectTotalUsage Zero = new(IdOf<Project>.None, 0, [], UtcDateTime.MinValue);

    public virtual bool Equals(ProjectTotalUsage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ProjectId.Equals(other.ProjectId) &&
               TotalMinutes == other.TotalMinutes &&
               LastUsed.Equals(other.LastUsed) &&
               Activities.SequenceEqual(other.Activities);
    }

    public override int GetHashCode()
        => HashCode.Combine(ProjectId);
}

public class UsageOverTime(
    Date from,
    Date to,
    int totalMinutes,
    IReadOnlyDictionary<Date, IList<ProjectTotalUsage>> usage)
{
    public Date From { get; } = from;
    public Date To { get; } = to;
    public int TotalMinutes { get; } = totalMinutes;
    public IReadOnlyDictionary<Date, IList<ProjectTotalUsage>> Usage { get; } = usage;
}
