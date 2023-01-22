using Quarter.Core.Models;
// ReSharper disable InconsistentNaming

namespace Quarter.HttpApi.Resources;

public record TimeSlot(string projectId, string activityId, int offset, int duration)
{
    public static TimeSlot From(ActivityTimeSlot slot)
        => new TimeSlot(slot.ProjectId.AsString(), slot.ActivityId.AsString(), slot.Offset, slot.Duration);
}

public record TimesheetResourceOutput(string date, int totalMinutes, IList<TimeSlot> timeSlots)
{
    public static TimesheetResourceOutput From(Timesheet timesheet)
    {
        var slots = timesheet.Slots().Select(TimeSlot.From).ToList();
        return new TimesheetResourceOutput(timesheet.Date.IsoString(),
            timesheet.TotalMinutes(),
            slots);
    }
}