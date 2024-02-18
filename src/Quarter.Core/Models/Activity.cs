using System;
using System.Text.Json.Serialization;
using Quarter.Core.Utils;

namespace Quarter.Core.Models;

public class Activity : IAggregate<Activity>
{
    [JsonConverter(typeof(IdOfJsonConverter<Activity>))]
    public IdOf<Activity> Id { get; set; }

    [JsonConverter(typeof(IdOfJsonConverter<Project>))]
    public IdOf<Project> ProjectId { get; set; }

    public UtcDateTime Created { get; set; }
    public UtcDateTime? Updated { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Color Color { get; set; }
    public bool IsArchived { get; set; }

    public Activity(IdOf<Project> projectId, string name, string description, Color color)
    {
        ProjectId = projectId;
        Id = IdOf<Activity>.Random();
        Created = UtcDateTime.Now();
        Name = name;
        Description = description;
        Color = color;
    }

#pragma warning disable CS8618
    public Activity()
    {
        // Deserialization purposes
    }
#pragma warning restore CS8618

    public void Archive()
        => IsArchived = true;

    public void Restore()
        => IsArchived = false;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        var other = (Activity)obj;
        return Id.Equals(other.Id) &&
               Name == other.Name &&
               Description == other.Description &&
               Created.DateTime == other.Created.DateTime &&
               Updated?.DateTime == other.Updated?.DateTime &&
               Color.Equals(other.Color) &&
               IsArchived == other.IsArchived;
    }

    public override int GetHashCode()
        => HashCode.Combine(Id);
}
