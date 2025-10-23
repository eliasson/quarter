using System;
using System.ComponentModel.DataAnnotations;

namespace Quarter.State.Forms;

public class ProjectFormData
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        var other = (ProjectFormData)obj;
        return Name == other.Name && Description == other.Description;
    }

    public override int GetHashCode()
        => HashCode.Combine(Name, Description);
}
