using System.ComponentModel.DataAnnotations;

namespace Quarter.HttpApi.Resources;

public class TimesheetResourceInput
{
    [Required]
    [RegularExpression("^\\d{4}-\\d{2}-\\d{2}$", ErrorMessage = "The date must be given in ISO-8601 (YYYY-MM-DD).")]
    public string? date { get; set; }
}