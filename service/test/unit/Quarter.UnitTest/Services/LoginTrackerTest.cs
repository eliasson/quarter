using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Services;

[TestFixture]
public class LoginTrackerTest
{
    private IRepositoryFactory _repositoryFactory = null!;
    private ControllableTimeProvider _timeProvider = null!;
    private LoginTracker _tracker = null!;
    private User _user = null!;

    [SetUp]
    public async Task Setup()
    {
        _repositoryFactory = new InMemoryRepositoryFactory();
        _timeProvider = new ControllableTimeProvider();
        _tracker = new LoginTracker(_repositoryFactory, _timeProvider, NullLogger<LoginTracker>.Instance);
        _user = await _repositoryFactory.UserRepository().CreateAsync(new User(new Email("jane.doe@example.com")), CancellationToken.None);
    }

    [Test]
    public async Task ItShouldRecordSignIn()
    {
        await _tracker.RecordLoginInAsync(_user.Id, CancellationToken.None);

        Assert.That(await LastLogin(), Is.EqualTo(_timeProvider.GetUtcNow().UtcDateTime));
    }

    [Test]
    public async Task ItShouldAlwaysRecordSignIn()
    {
        await _tracker.RecordLoginInAsync(_user.Id, CancellationToken.None);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        await _tracker.RecordLoginInAsync(_user.Id, CancellationToken.None);

        Assert.That(await LastLogin(), Is.EqualTo(_timeProvider.GetUtcNow().UtcDateTime));
    }

    [Test]
    public async Task ItShouldRecordFirstActivity()
    {
        await _tracker.RecordActivityAsync(_user.Id, CancellationToken.None);

        Assert.That(await LastLogin(), Is.EqualTo(_timeProvider.GetUtcNow().UtcDateTime));
    }

    [Test]
    public async Task ItShouldNotRecordActivityWithinInterval()
    {
        var timestamp = _timeProvider.GetUtcNow().UtcDateTime;
        await _tracker.RecordActivityAsync(_user.Id, CancellationToken.None);

        _timeProvider.Advance(LoginTracker.ActivityInterval - TimeSpan.FromMinutes(1));
        await _tracker.RecordActivityAsync(_user.Id, CancellationToken.None);

        Assert.That(await LastLogin(), Is.EqualTo(timestamp));
    }

    [Test]
    public async Task ItShouldRecordActivityAfterInterval()
    {
        await _tracker.RecordActivityAsync(_user.Id, CancellationToken.None);

        _timeProvider.Advance(LoginTracker.ActivityInterval);

        await _tracker.RecordActivityAsync(_user.Id, CancellationToken.None);

        Assert.That(await LastLogin(), Is.EqualTo(_timeProvider.GetUtcNow().UtcDateTime));
    }

    [Test]
    public async Task ItShouldNotThrowWhenRecordingFails()
    {
        var tracker = new LoginTracker(new FailingRepositoryFactory(), _timeProvider, NullLogger<LoginTracker>.Instance);

        await Assert.DoesNotThrowAsync(() => tracker.RecordLoginInAsync(_user.Id, CancellationToken.None));
    }

    private async Task<DateTime?> LastLogin()
    {
        var lastLogins = await _repositoryFactory.UserRepository().GetLastLoginsAsync(CancellationToken.None);
        return lastLogins.TryGetValue(_user.Id, out var ts) ? ts.DateTime : null;
    }

    private class FailingRepositoryFactory : IRepositoryFactory
    {
        public IUserRepository UserRepository() => throw new InvalidOperationException("Database is down");
        public IProjectRepository ProjectRepository(IdOf<User> userId) => throw new NotImplementedException();
        public IActivityRepository ActivityRepository(IdOf<User> userId) => throw new NotImplementedException();
        public ITimesheetRepository TimesheetRepository(IdOf<User> userId) => throw new NotImplementedException();
    }
}
