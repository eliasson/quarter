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

public abstract class RepositoryAccessTestBase
{
    protected abstract IRepositoryFactory Factory { get; }

    private readonly IdOf<User> _userIdOne = IdOf<User>.Random();
    private readonly IdOf<User> _userIdTwo = IdOf<User>.Random();

    private IUserRepository _userRepository;
    private IProjectRepository _projectRepositoryOne;
    private IProjectRepository _projectRepositoryTwo;
    private IActivityRepository _activityRepositoryOne;
    private IActivityRepository _activityRepositoryTwo;
    private ITimesheetRepository _timesheetRepositoryOne;
    private ITimesheetRepository _timesheetRepositoryTwo;

    [SetUp]
    public async Task Setup()
    {
        _userRepository = Factory.UserRepository();
        _projectRepositoryOne = Factory.ProjectRepository(_userIdOne);
        _projectRepositoryTwo = Factory.ProjectRepository(_userIdTwo);
        _activityRepositoryOne = Factory.ActivityRepository(_userIdOne);
        _activityRepositoryTwo = Factory.ActivityRepository(_userIdTwo);
        _timesheetRepositoryOne = Factory.TimesheetRepository(_userIdOne);
        _timesheetRepositoryTwo = Factory.TimesheetRepository(_userIdTwo);

        await _userRepository.Truncate(CancellationToken.None);
        await _projectRepositoryOne.Truncate(CancellationToken.None);
        await _projectRepositoryTwo.Truncate(CancellationToken.None);
        await _activityRepositoryOne.Truncate(CancellationToken.None);
        await _activityRepositoryTwo.Truncate(CancellationToken.None);
        await _timesheetRepositoryOne.Truncate(CancellationToken.None);
        await _timesheetRepositoryTwo.Truncate(CancellationToken.None);
    }

    [Test]
    public async Task GetAllShouldReturnProjectsForUserOnly()
    {
        await _projectRepositoryOne.CreateAsync(ArbitraryProject(), CancellationToken.None);
        await _projectRepositoryTwo.CreateAsync(ArbitraryProject(), CancellationToken.None);

        var oneIds = await _projectRepositoryOne.GetAllAsync(CancellationToken.None).Select(p => p.Id).ToListAsync(CancellationToken.None);
        var twoIds = await _projectRepositoryTwo.GetAllAsync(CancellationToken.None).Select(p => p.Id).ToListAsync(CancellationToken.None);

        Assert.That(oneIds.Except(twoIds), Is.EquivalentTo(oneIds));
    }

    [Test]
    public async Task GetAllShouldReturnActivitiesForUserOnly()
    {
        await _activityRepositoryOne.CreateAsync(ArbitraryActivity(), CancellationToken.None);
        await _activityRepositoryTwo.CreateAsync(ArbitraryActivity(), CancellationToken.None);

        var oneIds = await _activityRepositoryOne.GetAllAsync(CancellationToken.None).Select(p => p.Id).ToListAsync(CancellationToken.None);
        var twoIds = await _activityRepositoryTwo.GetAllAsync(CancellationToken.None).Select(p => p.Id).ToListAsync(CancellationToken.None);

        Assert.That(oneIds.Except(twoIds), Is.EquivalentTo(oneIds));
    }

    [Test]
    public async Task GetByIdShouldNotReturnProjectForOtherUser()
    {
        var projectOne = await _projectRepositoryOne.CreateAsync(ArbitraryProject(), CancellationToken.None);

        Assert.ThrowsAsync<NotFoundException>(() => _projectRepositoryTwo.GetByIdAsync(projectOne.Id, CancellationToken.None));
    }

    [Test]
    public async Task GetByIdShouldNotReturnActivityForOtherUser()
    {
        var activityOne = await _activityRepositoryOne.CreateAsync(ArbitraryActivity(), CancellationToken.None);

        Assert.ThrowsAsync<NotFoundException>(() => _activityRepositoryTwo.GetByIdAsync(activityOne.Id, CancellationToken.None));
    }

    [Test]
    public async Task UpdateByIdShouldNotFindProjectForOtherUser()
    {
        var projectOne = await _projectRepositoryOne.CreateAsync(ArbitraryProject(), CancellationToken.None);

        Assert.ThrowsAsync<NotFoundException>(() => _projectRepositoryTwo.UpdateByIdAsync(projectOne.Id, a => a, CancellationToken.None));
    }

    [Test]
    public async Task UpdateByIdShouldNotFindActivityForOtherUser()
    {
        var activityOne = await _activityRepositoryOne.CreateAsync(ArbitraryActivity(), CancellationToken.None);

        Assert.ThrowsAsync<NotFoundException>(() => _activityRepositoryTwo.UpdateByIdAsync(activityOne.Id, a => a, CancellationToken.None));
    }

    [Test]
    public async Task RemoveByIdDoesNotRemoveProjectForOtherUser()
    {
        var projectOne = await _projectRepositoryOne.CreateAsync(ArbitraryProject(), CancellationToken.None);
        await _projectRepositoryTwo.RemoveByIdAsync(projectOne.Id, CancellationToken.None);
        var readOne = await _projectRepositoryOne.GetByIdAsync(projectOne.Id, CancellationToken.None);

        Assert.That(readOne, Is.EqualTo(projectOne));
    }

    [Test]
    public async Task RemoveByIdDoesNotRemoveActivityForOtherUser()
    {
        var activityOne = await _activityRepositoryOne.CreateAsync(ArbitraryActivity(), CancellationToken.None);
        await _activityRepositoryTwo.RemoveByIdAsync(activityOne.Id, CancellationToken.None);
        var readOne = await _activityRepositoryOne.GetByIdAsync(activityOne.Id, CancellationToken.None);

        Assert.That(readOne, Is.EqualTo(activityOne));
    }

    [Test]
    public async Task GetAllShouldReturnTimesheetForUserOnly()
    {
        await _timesheetRepositoryOne.CreateAsync(ArbitraryTimesheet(), CancellationToken.None);
        await _timesheetRepositoryTwo.CreateAsync(ArbitraryTimesheet(), CancellationToken.None);

        var oneIds = await _timesheetRepositoryOne.GetAllAsync(CancellationToken.None).Select(ts => ts.Id).ToListAsync(CancellationToken.None);
        var twoIds = await _timesheetRepositoryTwo.GetAllAsync(CancellationToken.None).Select(ts => ts.Id).ToListAsync(CancellationToken.None);

        Assert.That(oneIds.Except(twoIds), Is.EquivalentTo(oneIds));
    }

    [Test]
    public async Task GetByDateShouldReturnTimesheetForUserOnly()
    {
        // Create two timesheets, where only the first has any time registered
        var date = Date.Random();

        var tsOne = Timesheet.CreateForDate(date);
        tsOne.Register(new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 0, 4));

        var tsTwo = Timesheet.CreateForDate(date);

        await _timesheetRepositoryOne.CreateAsync(tsOne, CancellationToken.None);
        await _timesheetRepositoryTwo.CreateAsync(tsTwo, CancellationToken.None);

        var readOne = await _timesheetRepositoryOne.GetByDateAsync(date, CancellationToken.None);
        var readTwo = await _timesheetRepositoryTwo.GetByDateAsync(date, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(readOne.Id, Is.Not.EqualTo(readTwo.Id));
            Assert.That(readOne.TotalMinutes(), Is.EqualTo(4 * 15));
            Assert.That(readTwo.TotalMinutes(), Is.Zero);
        });
    }

    private static Project ArbitraryProject()
        => new Project("Arbitrary", "Arbitrary");

    private static Activity ArbitraryActivity()
        => new Activity(IdOf<Project>.Random(), "Arbitrary", "Arbitrary", Color.FromHexString("#000"));

    private static Timesheet ArbitraryTimesheet()
        => Timesheet.CreateForDate(Date.Random());
}

public class InMemoryRepositoryAccessTest : RepositoryAccessTestBase
{
    protected override IRepositoryFactory Factory { get; } = new InMemoryRepositoryFactory();
}

[Category(TestCategories.DatabaseDependency)]
public class SqliteRepositoryAccessTest : RepositoryAccessTestBase
{
    protected override IRepositoryFactory Factory { get; } = new PostgresRepositoryFactory(UnitTestPostgresConnectionProvider.Instance);
}