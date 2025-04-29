using System;
using System.Collections.Generic;
using System.Linq;
using Quarter.Core.Models;

namespace Quarter.Core.Utils;

/// <summary>
/// Operation context contains contextual information for commands, queries, etc.
/// </summary>
/// <param name="UserId">The ID of the user issuing an operation.</param>
/// <param name="UserId">The roles of the user issuing an operation.</param>
public record OperationContext(IdOf<User> UserId, IReadOnlyCollection<UserRole> Roles)
{
    public static readonly OperationContext None = new OperationContext(IdOf<User>.Of(Guid.Empty.ToString()), []);

    public bool IsNone => UserId.Id == Guid.Empty;

    public bool HasRole(UserRole role) => Roles.Contains(role);

    public virtual bool Equals(OperationContext? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return UserId.Equals(other.UserId) && Roles.SequenceEqual(other.Roles);
    }

    public override int GetHashCode()
        => HashCode.Combine(UserId, Roles);
}
