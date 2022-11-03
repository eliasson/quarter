using System.ComponentModel.DataAnnotations;
using Quarter.Core.Models;

namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming

/// <summary>
/// Output resource for project aggregates
/// </summary>
/// <param name="id">The ID of the project</param>
/// <param name="name">The name of the project</param>
public record ProjectResourceOutput(string id, string name, string description, string created)
{
    public Uri Location()
        => new ($"/api/project/{id}", UriKind.Relative);

    public static ProjectResourceOutput From(Project project)
    {
        return new ProjectResourceOutput(project.Id.AsString(), project.Name, project.Description, project.Created.IsoString());
    }
}

public class ProjectResourceInput
{
    [Required]
    public string? name { get; set; }

    [Required]
    public string? description { get; set; }
}