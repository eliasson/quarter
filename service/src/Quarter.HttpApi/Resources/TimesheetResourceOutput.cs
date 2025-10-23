using Quarter.Core.Models;

// ReSharper disable InconsistentNaming

namespace Quarter.HttpApi.Resources;

public record TimeSlotOutput(string projectId, string activityId, int offset, int duration)
{
    public static TimeSlotOutput From(ActivityTimeSlot slot)
        => new(slot.ProjectId.AsString(), slot.ActivityId.AsString(), slot.Offset, slot.Duration);
}

public record TimesheetResourceOutput(string date, int totalMinutes, IList<TimeSlotOutput> timeSlots)
{
    public Uri Location()
        => new($"/api/timesheets/{date}", UriKind.Relative);

    public static TimesheetResourceOutput From(Timesheet timesheet)
    {
        var slots = timesheet.Slots().Select(TimeSlotOutput.From).ToList();
        return new TimesheetResourceOutput(timesheet.Date.IsoString(),
            timesheet.TotalMinutes(),
            slots);
    }
}
