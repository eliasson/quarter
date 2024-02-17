using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Quarter.Core.Utils;

namespace Quarter.Core.Models;

public struct ProjectSummary
{
    public IdOf<Project> ProjectId;
    public int Duration;
    public ActivitySummary[] Activities;
}

public struct ActivitySummary
{
    public IdOf<Activity> ActivityId;
    public int Duration;
}

public class Timesheet : IAggregate<Timesheet>
{
    /// <summary>
    /// The unique ID for this timesheet.
    /// </summary>
    [JsonConverter(typeof(IdOfJsonConverter<Timesheet>))]
    public IdOf<Timesheet> Id { get; set; } = null!;

    /// <summary>
    /// The date and time where the timesheet was created.
    /// </summary>
    public UtcDateTime Created { get; set; }

    /// <summary>
    /// The date and time where the timesheet was last updated.
    /// </summary>
    public UtcDateTime? Updated { get; set; }

    /// <summary>
    /// The date this timesheet is representing
    /// </summary>
    public Date Date { get; set; }

    /// <summary>
    /// The total number of activity minutes registered for this timesheet (regardless of activity).
    /// </summary>
    public int TotalMinutes()
        => _slots.Sum(s => s.Duration) * 15;

    /// <summary>
    /// Get the registered slots
    /// </summary>
    private IList<ActivityTimeSlot> _slots = new List<ActivityTimeSlot>();

    public IEnumerable<ActivityTimeSlot> Slots()
        => _slots;

    internal void SetSlots(IList<ActivityTimeSlot> slots)
        => _slots = slots;

    /// <summary>
    /// The first hour (0-23) that is in user in this timesheet. Null if no time is registered.
    /// </summary>
    public int? FirstHourInUse { get; set; }

    /// <summary>
    /// The last hour (0-23) that is in user in this timesheet. Null if no time is registered.
    /// </summary>
    public int? LastHourInUse { get; set; }

    private Timesheet(IdOf<Timesheet> timesheetId, Date date)
    {
        Id = timesheetId;
        Date = date;
        Created = UtcDateTime.Now();
    }

    [JsonConstructor]
    public Timesheet()
    {
    }

    public static Timesheet CreateForDate(IdOf<Timesheet> timesheetId, Date date)
        => new Timesheet(timesheetId, date);

    public static Timesheet CreateForDate(Date date)
        => new Timesheet(IdOf<Timesheet>.Random(), date);

    /// <summary>
    /// Register an activity time slot on this time sheet. This will replace any previously registered time
    /// for the same quarters. If any adjacent slots are for the same activity, the slots will be merged into a
    /// single slot.
    /// </summary>
    /// <param name="slot"></param>
    public void Register(TimeSlot slot)
    {
        // If the new slot completely overlaps the current slot, drop the existing
        // If the new slot is adjacent with the current, update the new slot and drop the existing
        // If the new slot partially overlaps with the current, update the new slot and drop the existing
        // Add the new slot after all existing slots have been processed since it might be updated multiple times

        var updated = new List<ActivityTimeSlot>();

        foreach (var current in _slots)
        {
            var drop = false;
            var isSameActivity = false;

            if (slot is ActivityTimeSlot activityTimeSlot)
                isSameActivity = current.ActivityId == activityTimeSlot.ActivityId;

            if (current.Offset >= slot.Offset && current.EndsAt <= slot.EndsAt)
            {
                // Completely overlaps (i.e. replaces)
                drop = true;
            }
            else if (current.EndsAt == slot.Offset && isSameActivity)
            {
                // Just before (i.e. merge)
                slot.Offset = current.Offset;
                slot.Duration += current.Duration;
                drop = true;
            }
            else if (current.Offset == slot.EndsAt && isSameActivity)
            {
                // Just after (i.e. merge)
                slot.Duration += current.Duration;
                drop = true;
            }
            else if (current.EndsAt > slot.Offset && current.EndsAt <= slot.EndsAt)
            {
                // Slot overlaps tail part of current
                if (isSameActivity)
                {
                    // Merge into one slot
                    var delta = Math.Abs(slot.Offset - current.EndsAt);
                    slot.Duration = slot.Duration + (current.Duration - delta);
                    slot.Offset = current.Offset;
                    drop = true;
                }
                else
                {
                    // Shrink the current (old) slot
                    var delta = Math.Abs(slot.Offset - current.EndsAt);
                    current.Duration -= delta;
                }
            }
            else if (current.Offset >= slot.Offset && current.Offset < slot.EndsAt)
            {
                // Slot overlaps head part of current
                if (isSameActivity)
                {
                    // Merge into one slot
                    var delta = current.EndsAt - slot.EndsAt;
                    slot.Duration = slot.Duration + delta;
                    drop = true;
                }
                else
                {
                    // Shrink the current (old) slot
                    var delta = slot.EndsAt - current.Offset;
                    current.Offset = slot.EndsAt;
                    current.Duration -= delta;
                }
            }
            else if (current.Offset <= slot.Offset && current.EndsAt >= slot.EndsAt)
            {
                // The new slot is a subset of the existing slot
                if (isSameActivity)
                {
                    slot.Offset = current.Offset;
                    slot.Duration = current.Duration;
                    drop = true;
                }
                else
                {
                    // Shrink the current (old) slot then add a new slot representing the old tail
                    var initialDuration = current.Duration;
                    current.Duration = slot.Offset - current.Offset;
                    var tailDuration = initialDuration - current.Duration - slot.Duration;
                    var tailSlot = new ActivityTimeSlot(current.ProjectId, current.ActivityId, slot.EndsAt, tailDuration);
                    updated.Add(tailSlot);
                }
            }

            if (!drop) updated.Add(current);
        }

        if (slot is ActivityTimeSlot aSlot)
            updated.Add(aSlot);

        _slots = updated.OrderBy(s => s.Offset).ToList();

        if (!_slots.Any())
        {
            FirstHourInUse = null;
            LastHourInUse = null;
        }
        else
        {
            var firstSlot = _slots.First();
            FirstHourInUse = firstSlot.Offset / 4;

            var lastSlot = _slots.Last();
            LastHourInUse = (lastSlot.Offset + lastSlot.Duration) / 4;
        }
    }

    /// <summary>
    /// Summarize the project totals per project and activity.
    /// </summary>
    /// <returns>The project summary usage</returns>
    public IEnumerable<ProjectSummary> Summarize()
    {
        var activityDuration = _slots.Aggregate(new Dictionary<IdOf<Activity>, (IdOf<Project>, int)>(), (acc, slot) =>
        {
            if (!acc.ContainsKey(slot.ActivityId))
                acc.Add(slot.ActivityId, (slot.ProjectId, 0));
            acc[slot.ActivityId] = (slot.ProjectId, acc[slot.ActivityId].Item2 + slot.Duration);
            return acc;
        }).ToList();

        var projects = activityDuration.Aggregate(new Dictionary<IdOf<Project>, List<ActivitySummary>>(), (acc, t) =>
        {
            if (!acc.ContainsKey(t.Value.Item1))
                acc.Add(t.Value.Item1, new List<ActivitySummary>());
            acc[t.Value.Item1].Add(new ActivitySummary { ActivityId = t.Key, Duration = t.Value.Item2 });
            return acc;
        });

        return projects.Select(kvp =>
        {
            var duration = kvp.Value.Aggregate(0, (acc, item) => acc += item.Duration);
            return new ProjectSummary { ProjectId = kvp.Key, Activities = kvp.Value.ToArray(), Duration = duration };
        });
    }

    public Timesheet WithCreatedTimestamp(UtcDateTime utcDateTime)
    {
        Created = utcDateTime;
        return this;
    }

    public Timesheet WithUpdatedTimestamp(UtcDateTime utcDateTime)
    {
        Updated = utcDateTime;
        return this;
    }

    public override bool Equals(object? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (other is Timesheet otherSheet)
        {
            var fastCompare = Id.Equals(otherSheet.Id) && Date.Equals(otherSheet.Date);
            if (!fastCompare) return fastCompare;
            return _slots.SequenceEqual(otherSheet._slots);
        }

        return false;
    }

    public override int GetHashCode()
        => HashCode.Combine(Id, Date, _slots.GetHashCode());
}
