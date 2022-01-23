using System.Collections.Generic;
using Quarter.Core.Models;

namespace Quarter.Core.Events
{
    public record UserCreatedEvent(User User) : IEvent;

    public record UserRemovedEvent(IdOf<User> UserId) : IEvent;

    public record AssignedUserRoleEvent(IdOf<User> UserId, IEnumerable<UserRole> Roles) : IEvent;

    public record RevokedUserRoleEvent(IdOf<User> UserId, IEnumerable<UserRole> Roles) : IEvent;

    public record ProjectCreatedEvent(Project Project) : IEvent;

    public record ProjectRemovedEvent(IdOf<Project> ProjectId) : IEvent;

    public record ProjectEditedEvent(IdOf<Project> ProjectId) : IEvent;

    public record ActivityCreatedEvent(Activity Activity) : IEvent;

    public record ActivityEditedEvent(IdOf<Activity> ActivityId) : IEvent;

    public record ActivityRemovedEvent(IdOf<Activity> ActivityId) : IEvent;
}