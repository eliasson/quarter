using System.ComponentModel.DataAnnotations;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.Resources;

public class TimesheetResourceInput
{
    [Required]
    [RegularExpression("^\\d{4}-\\d{2}-\\d{2}$", ErrorMessage = "The date must be given in ISO-8601 (YYYY-MM-DD).")]
    public string? date { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "The timeSlots field must not be empty.")]
    public TimeSlotInput[]? timeSlots { get; set; }

    public Date ToDate()
    {
        if (date is null) throw new InvalidOperationException("Cannot convert date when missing");
        return Date.From(date);
    }

    public IEnumerable<TimeSlot> ToSlots()
    {
        if (timeSlots is null)
            return Enumerable.Empty<TimeSlot>();

        return timeSlots.Select(s => s.ToSlot()).Where(s => s is not null)!;
    }
}

public class TimeSlotInput
{
    [Required]
    public string? projectId { get; set; }

    [Required]
    public string? activityId { get; set; }

    [Range(0, 95)]
    public int? offset { get; set; }

    [Range(1, 96)]
    public int? duration { get; set; } // TODO: This should be validated together with the offset

    public TimeSlot? ToSlot()
    {
        if (projectId is null || activityId is null || offset is null || duration is null)
            return null;

        return  new ActivityTimeSlot(IdOf<Project>.Of(projectId), IdOf<Activity>.Of(activityId), offset.Value, duration.Value);
    }
}