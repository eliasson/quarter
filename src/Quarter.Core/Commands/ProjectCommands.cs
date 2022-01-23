using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Commands;

public record AddProjectCommand(string Name, string Description) : ICommand;
public record EditProjectCommand(IdOf<Project> ProjectId, string? Name, string? Description) : ICommand;
public record RemoveProjectCommand(IdOf<Project> ProjectId) : ICommand;

public record AddActivityCommand(IdOf<Project> ProjectId, string Name, string Description, Color Color) : ICommand;
public record EditActivityCommand(IdOf<Activity> ActivityId, string? Name, string? Description, Color? Color) : ICommand;
public record RemoveActivityCommand(IdOf<Activity> ActivityId) : ICommand;