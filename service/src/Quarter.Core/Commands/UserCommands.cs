using System;
using System.Collections.Generic;
using System.Linq;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Commands;

public record AddUserCommand(Email Email, IEnumerable<UserRole> Roles) : ICommand
{
    public virtual bool Equals(AddUserCommand? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Email.Equals(other.Email) && Roles.SequenceEqual(other.Roles);
    }

    public override int GetHashCode()
        => HashCode.Combine(Email, Roles);
}

