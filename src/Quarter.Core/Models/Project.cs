using System;
using System.Text.Json.Serialization;
using Quarter.Core.Utils;

namespace Quarter.Core.Models
{
    public class Project : IAggregate<Project>
    {
        [JsonConverter(typeof(IdOfJsonConverter<Project>))]
        public IdOf<Project> Id { get; set; }
        public UtcDateTime Created { get; set; }
        public UtcDateTime? Updated { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsArchived { get; set; }

        public Project(string name, string description)
        {
            Id = IdOf<Project>.Random();
            Created = UtcDateTime.Now();
            Name = name;
            Description = description;
        }

#pragma warning disable CS8618
        public Project()
        {
            // Deserialization
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

            var other = (Project)obj;
            return Name == other.Name &&
                   Description == other.Description &&
                   Created.DateTime == other.Created.DateTime &&
                   Updated?.DateTime == other.Updated?.DateTime &&
                   IsArchived == other.IsArchived;
        }

        public override int GetHashCode()
            => HashCode.Combine(Id, Name, Description, Created, Updated, IsArchived);
    }
}
