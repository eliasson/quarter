using System;
using Quarter.Core.Models;

namespace Quarter.Core.Utils;

/// <summary>
/// Operation context contains contextual information for commands, queries, etc.
/// </summary>
/// <param name="UserId"></param>
public record OperationContext(IdOf<User> UserId)
{
    public static readonly OperationContext None = new OperationContext(IdOf<User>.Of(Guid.Empty.ToString()));

    public bool IsNone => UserId.Id == Guid.Empty;
}
