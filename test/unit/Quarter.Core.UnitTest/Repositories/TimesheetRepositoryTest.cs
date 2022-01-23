using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.UnitTest.TestUtils;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Repositories;

public abstract class TimesheetRepositoryTest : RepositoryTestBase<Timesheet, ITimesheetRepository>
{
    private IdOf<Project> _stableProjectId = IdOf<Project>.Random();
    private IdOf<Activity> _stableActivityId = IdOf<Activity>.Random();

    protected override IdOf<Timesheet> ArbitraryId()
        => IdOf<Timesheet>.Random();

    protected override Timesheet ArbitraryAggregate()
        => Timesheet.CreateForDate(Date.Random());

    protected override Timesheet WithoutTimestamps(Timesheet aggregate)
        => aggregate;

    protected override Timesheet Mutate(Timesheet aggregate)
    {
        aggregate.Register(new ActivityTimeSlot(_stableProjectId, _stableActivityId, 0, 2));
        return aggregate;
    }

    [SetUp]
    public void CreateIds()
    {
        _stableProjectId = IdOf<Project>.Random();
        _stableActivityId = IdOf<Activity>.Random();
    }

    [Test]
    public async Task GetByDateShouldReturnTimesheetWhenExisting()
    {
        var repository = Repository();
        var agg = ArbitraryAggregate();

        var storedAgg = await repository.CreateAsync(agg, CancellationToken.None);
        var readAgg = await repository.GetByDateAsync(storedAgg.Date, CancellationToken.None);

        Assert.That(WithoutTimestamps(readAgg), Is.EqualTo(WithoutTimestamps(agg)));
    }

    [Test]
    public void GetByDateShouldThrowWhenTimesheetMissing()
    {
        var repository = Repository();
        var strayDate = new Date(DateTime.Parse("1998-06-22T00:00:00Z"));

        var ex = Assert.CatchAsync<NotFoundException>(() =>
            repository.GetByDateAsync(strayDate, CancellationToken.None));
        Assert.That(ex?.Message, Does.Contain("No timesheet for date 1998-06-22 exists"));
    }

    [Test]
    public async Task ShouldStoreRegisteredTimeSlotsWhenCreating()
    {
        var repository = Repository();
        var agg = ArbitraryAggregate();
        agg.Register(new ActivityTimeSlot(_stableProjectId,_stableActivityId, 0, 2));
        agg.Register(new ActivityTimeSlot(_stableProjectId,_stableActivityId, 4, 2));

        var storedAgg = await repository.CreateAsync(agg, CancellationToken.None);
        var readAgg = await repository.GetByDateAsync(storedAgg.Date, CancellationToken.None);

        Assert.That(readAgg.Slots, Is.EqualTo(new []
        {
            new ActivityTimeSlot(_stableProjectId,_stableActivityId, 0, 2),
            new ActivityTimeSlot(_stableProjectId,_stableActivityId, 4, 2),
        }));
    }

    [Test]
    public async Task ShouldStoreRegisteredTimeSlotsWhenUpdating()
    {
        var repository = Repository();
        var agg = ArbitraryAggregate();

        var storedAgg = await repository.CreateAsync(agg, CancellationToken.None);
        _ = await repository.UpdateByIdAsync(agg.Id,timesheet =>
        {
            timesheet.Register(new ActivityTimeSlot(_stableProjectId,_stableActivityId, 0, 2));
            timesheet.Register(new ActivityTimeSlot(_stableProjectId,_stableActivityId, 4, 2));
            return timesheet;
        }, CancellationToken.None);
        var readAgg = await repository.GetByDateAsync(storedAgg.Date, CancellationToken.None);

        Assert.That(readAgg.Slots, Is.EqualTo(new []
        {
            new ActivityTimeSlot(_stableProjectId,_stableActivityId, 0, 2),
            new ActivityTimeSlot(_stableProjectId,_stableActivityId, 4, 2),
        }));
    }

    [Test]
    public async Task ShouldReadRegisteredTimeSlotsGettingAll()
    {
        var repository = Repository();
        var agg = ArbitraryAggregate();
        agg.Register(new ActivityTimeSlot(_stableProjectId, _stableActivityId, 0, 2));
        agg.Register(new ActivityTimeSlot(_stableProjectId, _stableActivityId, 4, 2));

        _ = await repository.CreateAsync(agg, CancellationToken.None);
        var all = repository.GetAllAsync(CancellationToken.None);
        var readAgg = await all.FirstAsync();
        Assert.That(readAgg.Slots, Is.EqualTo(new []
        {
            new ActivityTimeSlot(_stableProjectId,_stableActivityId, 0, 2),
            new ActivityTimeSlot(_stableProjectId,_stableActivityId, 4, 2),
        }));
    }

    [Test]
    public async Task ShouldStoreSlotCreatedTimestamp()
    {
        var repository = Repository();
        var agg = ArbitraryAggregate();
        var registerTimestamp = DateTime.UtcNow;
        agg.Register(new ActivityTimeSlot(_stableProjectId,_stableActivityId, 0, 2));

        await Task.Delay(100);
        _ = await repository.CreateAsync(agg, CancellationToken.None);
        var readAgg = await repository.GetByIdAsync(agg.Id, CancellationToken.None);
        var slot = readAgg.Slots().First();

        Assert.That(slot.Created.DateTime - registerTimestamp, Is.LessThan(TimeSpan.FromMilliseconds(10)));
    }

    [Test]
    public async Task ShouldReportZeroTotalUsage()
    {
        var repository = Repository();
        var usage = await repository.GetProjectTotalUsageAsync(_stableProjectId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(usage.TotalMinutes, Is.Zero);
            Assert.That(usage.Activities, Is.Empty);
        });
    }

    [Test]
    public async Task ShouldReportTotalUsage()
    {
        var repository = Repository();
        var agg = ArbitraryAggregate();
        agg.Register(new ActivityTimeSlot(_stableProjectId, IdOf<Activity>.Random(), 0, 2));
        agg.Register(new ActivityTimeSlot(_stableProjectId, IdOf<Activity>.Random(), 4, 4));

        _ = await repository.CreateAsync(agg, CancellationToken.None);
        var usage = await repository.GetProjectTotalUsageAsync(_stableProjectId, CancellationToken.None);

        Assert.That(usage.TotalMinutes, Is.EqualTo(2 * 15  + 4 * 15));
    }

    [Test]
    public async Task ShouldReportTotalPerActivity()
    {
        var activityOne = IdOf<Activity>.Random();
        var activityTwo = IdOf<Activity>.Random();
        var repository = Repository();
        var agg = ArbitraryAggregate();
        var slotOne = new ActivityTimeSlot(_stableProjectId, activityOne, 0, 2);
        var slotTwo = new ActivityTimeSlot(_stableProjectId, activityTwo, 4, 4);
        agg.Register(slotOne);
        agg.Register(slotTwo);

        _ = await repository.CreateAsync(agg, CancellationToken.None);
        var usage = await repository.GetProjectTotalUsageAsync(_stableProjectId, CancellationToken.None);
        var activities = usage.Activities.Select(a => (a.ActivityId, a.TotalMinutes, a.LastUsed.DateTime.WithoutMilliseconds()));

        Assert.That(activities, Is.EquivalentTo(new[]
        {
            (activityOne, 2*15, slotOne.Created.DateTime.WithoutMilliseconds()),
            (activityTwo, 4*15, slotTwo.Created.DateTime.WithoutMilliseconds()),
        }));
    }

    [Test]
    public async Task ShouldRemoveSlotsForProject()
    {
        var projectIdOne = IdOf<Project>.Random();
        var projectIdTwo = IdOf<Project>.Random();
        var activityOne = IdOf<Activity>.Random();
        var activityTwo = IdOf<Activity>.Random();

        var repository = Repository();
        var agg = ArbitraryAggregate();
        agg.Register(new ActivityTimeSlot(projectIdOne, activityOne, 0, 2));
        agg.Register(new ActivityTimeSlot(projectIdTwo, activityTwo, 4, 4));

        var timesheet = await repository.CreateAsync(agg, CancellationToken.None);
        var removeResult = await repository.RemoveSlotsForProjectAsync(projectIdOne, CancellationToken.None);

        var readTimesheet = await repository.GetByIdAsync(timesheet.Id, CancellationToken.None);
        var slotProjects = readTimesheet.Slots().Select(s => s.ProjectId);

        Assert.Multiple(() =>
        {
            Assert.That(removeResult, Is.EqualTo(RemoveResult.Removed));
            Assert.That(slotProjects, Is.EqualTo(new [] { projectIdTwo }));
        });
    }

    [Test]
    public async Task ShouldNotRemoveAnySlotsForProject()
    {
        var repository = Repository();
        var removeResult = await repository.RemoveSlotsForProjectAsync(IdOf<Project>.Random(), CancellationToken.None);

        Assert.That(removeResult, Is.EqualTo(RemoveResult.NotRemoved));
    }

    [Test]
    public async Task ShouldRemoveSlotsForActivity()
    {
        var projectIdOne = IdOf<Project>.Random();
        var activityOne = IdOf<Activity>.Random();
        var activityTwo = IdOf<Activity>.Random();

        var repository = Repository();
        var agg = ArbitraryAggregate();
        agg.Register(new ActivityTimeSlot(projectIdOne, activityOne, 0, 2));
        agg.Register(new ActivityTimeSlot(projectIdOne, activityTwo, 4, 4));

        var timesheet = await repository.CreateAsync(agg, CancellationToken.None);
        var removeResult = await repository.RemoveSlotsForActivityAsync(activityOne, CancellationToken.None);

        var readTimesheet = await repository.GetByIdAsync(timesheet.Id, CancellationToken.None);
        var slotProjects = readTimesheet.Slots().Select(s => s.ActivityId);

        Assert.Multiple(() =>
        {
            Assert.That(removeResult, Is.EqualTo(RemoveResult.Removed));
            Assert.That(slotProjects, Is.EqualTo(new [] { activityTwo }));
        });
    }

    [Test]
    public async Task ShouldNotRemoveAnySlotsForActivity()
    {
        var repository = Repository();
        var removeResult = await repository.RemoveSlotsForActivityAsync(IdOf<Activity>.Random(), CancellationToken.None);

        Assert.That(removeResult, Is.EqualTo(RemoveResult.NotRemoved));
    }
}

public class InMemoryTimesheetRepositoryTest : TimesheetRepositoryTest
{
    protected override ITimesheetRepository Repository()
        => new InMemoryTimesheetRepository();
}

public class PostgresqlTimesheetRepositoryTest : TimesheetRepositoryTest
{
    protected override ITimesheetRepository Repository()
        => new PostgresTimesheetRepository(UnitTestPostgresConnectionProvider.Instance, IdOf<User>.Random());
}
