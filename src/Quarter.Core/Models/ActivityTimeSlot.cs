using System;
using System.Text.Json.Serialization;
using Quarter.Core.Utils;

namespace Quarter.Core.Models;

public class ActivityTimeSlot : TimeSlot
{
    /// <summary>
    /// The ID of the project this time slot represents
    /// </summary>
    [JsonConverter(typeof(IdOfJsonConverter<Project>))]
    public IdOf<Project> ProjectId { get; set; } = null!;

    /// <summary>
    /// The ID of the activity this time slot represents
    /// </summary>
    [JsonConverter(typeof(IdOfJsonConverter<Activity>))]
    public IdOf<Activity> ActivityId { get; set; } = null!;

    [JsonConstructor]
    public ActivityTimeSlot() : base()
    {
    }

    public ActivityTimeSlot(IdOf<Project> projectId, IdOf<Activity> activityId, int offset, int duration)
        : base(offset, duration)
    {

        ProjectId = projectId;
        ActivityId = activityId;
    }

    public ActivityTimeSlot(IdOf<Project> projectId, IdOf<Activity> activityId, int offset, int duration, UtcDateTime created)
        : base(offset, duration)
    {

        ProjectId = projectId;
        ActivityId = activityId;
        Created = created;
    }

    public override bool Equals(object? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other is ActivityTimeSlot otherSlot)
        {
            return ProjectId.Equals(otherSlot.ProjectId)
                   && ActivityId.Equals(otherSlot.ActivityId)
                   && Offset == otherSlot.Offset
                   && Duration == otherSlot.Duration;
        }

        return false;
    }

    public override int GetHashCode()
        => HashCode.Combine(ProjectId, ActivityId, Offset, Duration);
}
