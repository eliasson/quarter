using System.ComponentModel.DataAnnotations;
using Quarter.Core.Models;

namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming

/// <summary>
/// Output resource for project aggregates
/// </summary>
/// <param name="id">The ID of the project</param>
/// <param name="name">The name of the project</param>
/// <param name="description">The project description</param>
/// <param name="isArchived">Whether or not the project is archived</param>
/// <param name="created">Timestamp for when the project was created (ISO-8601)</param>
/// <param name="updated">Timestamp for when the project was last updated, or null if it has never been updated (ISO-8601)</param>
public record ProjectResourceOutput(string id, string name, string description, bool isArchived, string created, string? updated)
{
    public Uri Location()
        => new ($"/api/projects/{id}", UriKind.Relative);

    public static ProjectResourceOutput From(Project project)
    {
        return new ProjectResourceOutput(project.Id.AsString(), project.Name, project.Description, project.IsArchived, project.Created.IsoString(), project.Updated?.IsoString());
    }
}

public class CreateProjectResourceInput
{
    [Required]
    public string? name { get; set; }

    [Required]
    public string? description { get; set; }
}

public class UpdateProjectResourceInput
{
    public string? name { get; set; }

    public string? description { get; set; }
}