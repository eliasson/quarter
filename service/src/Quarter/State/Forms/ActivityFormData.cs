using System;
using System.ComponentModel.DataAnnotations;

namespace Quarter.State.Forms;

public class ActivityFormData
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Activity color is required")]
    [RegularExpression("^#([0-9a-fA-F]{3}){1,2}$", ErrorMessage = "Color must be a HEX value (e.g. #04a85b)")]
    public string Color { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        var other = (ActivityFormData)obj;
        return Name == other.Name && Description == other.Description && Color == other.Color;
    }

    public override int GetHashCode()
        => HashCode.Combine(Name, Description, Color);
}
