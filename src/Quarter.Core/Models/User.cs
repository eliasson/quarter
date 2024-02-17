using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Quarter.Core.Utils;

namespace Quarter.Core.Models;

public enum UserRole
{
    Administrator = 1,
}

public class User : IAggregate<User>
{
    public static readonly List<UserRole> NoRoles = new();

    [JsonConverter(typeof(IdOfJsonConverter<User>))]
    public IdOf<User> Id { get; set; }
    public UtcDateTime Created { get; set; }
    public UtcDateTime? Updated { get; set; }
    public Email Email { get; set; }
    public IList<UserRole> Roles { get; set; }

    public User(Email email) : this(email, new List<UserRole>())
    {
    }

    public User(Email email, IEnumerable<UserRole> roles)
    {
        Email = email;
        Id = IdOf<User>.Random();
        Created = UtcDateTime.Now();
        Roles = roles.ToList();
    }

#pragma warning disable CS8618
    public User()
    {
        // Deserialization purposes
    }
#pragma warning restore CS8618

    public bool IsAdmin()
        => Roles.Contains(UserRole.Administrator);

    // TODO: Make thread-safe
    public void AssignRole(UserRole role)
    {
        if (!Roles.Contains(role))
            Roles.Add(role);
    }

    // TODO: Make thread-safe
    public void RevokeRole(UserRole role)
    {
        if (Roles.Contains(role))
            Roles.Remove(role);
    }

    public static User StandardUser(Email email)
        => new(email, NoRoles);

    public static User AdminUser(Email email)
        => new(email, new[] { UserRole.Administrator }.ToList());

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        var other = (User)obj;
        return Id.Equals(other.Id) &&
               Email.Equals(other.Email) &&
               Roles.SequenceEqual(other.Roles);
    }

    public override int GetHashCode()
        => HashCode.Combine(Id);
}
