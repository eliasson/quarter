using System;
using System.ComponentModel.DataAnnotations;

namespace Quarter.State.Forms;

public class UserFormData
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        var other = (UserFormData) obj;
        return Email == other.Email && IsAdmin == other.IsAdmin;
    }

    public override int GetHashCode()
        => HashCode.Combine(Email, IsAdmin);
}