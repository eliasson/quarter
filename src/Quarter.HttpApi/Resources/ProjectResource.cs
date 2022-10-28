using Quarter.Core.Models;

namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming

/// <summary>
/// Output resource for project aggregates
/// </summary>
/// <param name="id">The ID of the project</param>
/// <param name="name">The name of the project</param>
public record ProjectResourceOutput(string id, string name, string description)
{
    public static ProjectResourceOutput From(Project project)
    {
        return new ProjectResourceOutput(project.Id.AsString(), project.Name, project.Description);
    }
}
