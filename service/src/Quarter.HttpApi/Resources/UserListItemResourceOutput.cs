using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming

/// <summary>
/// Output resource for a user in the (administrator) user listing.
/// </summary>
/// <param name="id">The ID of the user.</param>
/// <param name="email">The email address of the user.</param>
/// <param name="created">Timestamp for when the user was created (ISO-8601).</param>
/// <param name="updated">Timestamp for when the user was last updated, or null if it has never been updated (ISO-8601).</param>
/// <param name="roles">The roles assigned to the user (e.g. "administrator"), empty for a standard user.</param>
/// <param name="lastLogin">Timestamp for when the user last logged in or was active, or null if the user has never logged in (ISO-8601).</param>
public record UserListItemResourceOutput(string id, string email, string created, string? updated, IReadOnlyList<string> roles, string? lastLogin)
{
    public static UserListItemResourceOutput From(User user, UtcDateTime? lastLogin) =>
        new(
            user.Id.ToString(),
            user.Email.AsString(),
            user.Created.IsoString(),
            user.Updated?.IsoString(),
            user.Roles.Select(RoleName).ToList(),
            lastLogin?.IsoString());

    /// <summary>
    /// Explicit mapping so that renaming the enum never changes the API contract.
    /// </summary>
    private static string RoleName(UserRole role) =>
        role switch
        {
            UserRole.Administrator => "administrator",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Role has no API name"),
        };
}
