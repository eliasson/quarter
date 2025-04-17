using Quarter.Core.Models;

namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming

/// <summary>
/// Output resource for user aggregates
/// </summary>
/// <param name="id">The ID of the user</param>
/// <param name="email">The email address of the user</param>
/// <param name="created">Timestamp for when the user was created (ISO-8601)</param>
/// <param name="updated">Timestamp for when the user was last updated, or null if it has never been updated (ISO-8601)</param>
public record UserResourceOutput(string id, string email, string created, string? updated)
{
    public static UserResourceOutput From(User user) =>
        new(
            user.Id.ToString(),
            user.Email.AsString(),
            user.Created.IsoString(),
            user.Updated?.IsoString());
}
