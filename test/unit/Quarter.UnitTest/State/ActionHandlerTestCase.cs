using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public abstract class ActionHandlerTestCase
{
    protected readonly TestCommandHandler CommandHandler = new TestCommandHandler();
    protected readonly IRepositoryFactory RepositoryFactory = new InMemoryRepositoryFactory();
    protected readonly TestIUserAuthorizationService UserAuthorizationService = new TestIUserAuthorizationService();
    protected readonly ActionHandler ActionHandler;
    protected readonly IdOf<User> ActingUserId = IdOf<User>.Random();

    protected ActionHandlerTestCase()
    {
        UserAuthorizationService.UserId = ActingUserId;
        ActionHandler = new ActionHandler(RepositoryFactory, CommandHandler, UserAuthorizationService);
    }

    protected static ApplicationState NewState()
        => new ApplicationState();

    protected async Task<Project> AddProject(IdOf<User> userId, string name, string description)
    {
        var repo = RepositoryFactory.ProjectRepository(userId);
        return await repo.CreateAsync(new Project(name, description), CancellationToken.None);
    }

    protected async Task<Activity> AddActivity(IdOf<User> userId, IdOf<Project> projectId, string name, string description, Color color)
    {
        var repo = RepositoryFactory.ActivityRepository(userId);
        var a = new Activity(projectId, name, description, color);
        return await repo.CreateAsync(a, CancellationToken.None);
    }

    protected Task<Timesheet> AddTimesheet(IdOf<User> userId, Date date, int offset, int duration)
        => AddTimesheet(userId, date, IdOf<Project>.Random(), IdOf<Activity>.Random(), offset, duration);

    protected async Task<Timesheet> AddTimesheet(IdOf<User> userId, Date date, IdOf<Project> projectId, IdOf<Activity> activityId, int offset, int duration)
    {
        var repo = RepositoryFactory.TimesheetRepository(userId);
        var ts = Timesheet.CreateForDate(date);
        ts.Register(new ActivityTimeSlot(projectId, activityId, offset, duration));
        return await repo.CreateAsync(ts, CancellationToken.None);
    }

    protected void AssertIssuedCommand(ICommand expectedCommand)
    {
        var cmd = CommandHandler.LastExecutedCommand();
        Assert.That(cmd.Command, Is.EqualTo(expectedCommand));
    }

    protected void AssertIssuedCommandByUserId(ICommand expectedCommand, IdOf<User> userId)
    {
        var cmd = CommandHandler.LastExecutedCommand();
        Assert.Multiple(() =>
        {

            Assert.That(cmd.Command, Is.EqualTo(expectedCommand));
            Assert.That(cmd.Context.UserId, Is.EqualTo(userId));
        });
    }
}
