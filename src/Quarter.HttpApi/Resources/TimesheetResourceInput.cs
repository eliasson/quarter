using System.ComponentModel.DataAnnotations;

namespace Quarter.HttpApi.Resources;

public class TimesheetResourceInput
{
    [Required]
    [RegularExpression("^\\d{4}-\\d{2}-\\d{2}$", ErrorMessage = "The date must be given in ISO-8601 (YYYY-MM-DD).")]
    public string? date { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "The timeSlots field must not be empty.")]
    public TimeSlotInput[]? timeSlots { get; set; }
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
}