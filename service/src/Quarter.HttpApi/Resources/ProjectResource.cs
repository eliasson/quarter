using System.ComponentModel.DataAnnotations;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming

/// <summary>
/// Output resource for project aggregates
/// </summary>
/// <param name="id">The ID of the project</param>
/// <param name="name">The name of the project</param>
/// <param name="description">The project description</param>
/// <param name="color">The project color in CSS HEX</param>
/// <param name="isArchived">Whether or not the project is archived</param>
/// <param name="created">Timestamp for when the project was created (ISO-8601)</param>
/// <param name="updated">Timestamp for when the project was last updated, or null if it has never been updated (ISO-8601)</param>
public record ProjectResourceOutput(string id, string name, string description, string color, bool isArchived, string created, string? updated)
{
    public Uri Location()
        => new($"/api/projects/{id}", UriKind.Relative);

    public static ProjectResourceOutput From(Project project)
    {
        return new ProjectResourceOutput(project.Id.AsString(), project.Name, project.Description, project.Color.ToHex(), project.IsArchived, project.Created.IsoString(), project.Updated?.IsoString());
    }
}

public class CreateProjectResourceInput
{
    [Required]
    public string? name { get; set; }

    public string? description { get; set; }

    [RegularExpression("^#([0-9a-fA-F]{3}){1,2}$", ErrorMessage = "The color field is invalid, must be a HEX value (e.g. #04a85b).")]
    public string? color { get; set; }
}

public class UpdateProjectResourceInput
{
    public string? name { get; set; }

    public string? description { get; set; }

    [RegularExpression("^#([0-9a-fA-F]{3}){1,2}$", ErrorMessage = "The color field is invalid, must be a HEX value (e.g. #04a85b).")]
    public string? color { get; set; }

    public bool? isArchived { get; set; }
}
