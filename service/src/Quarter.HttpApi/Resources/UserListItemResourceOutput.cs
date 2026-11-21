using Quarter.Core.Models;

namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming

public record UserListItemResourceOutput(string id, string email, string created, string? updated)
{
    public static UserListItemResourceOutput From(User user) =>
        new(
            user.Id.ToString(),
            user.Email.AsString(),
            user.Created.IsoString(),
            user.Updated?.IsoString());
}
