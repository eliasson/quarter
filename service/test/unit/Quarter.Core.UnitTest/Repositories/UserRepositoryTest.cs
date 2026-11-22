using System;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using NUnit.Framework;
using Quarter.Core.UnitTest.TestUtils;

namespace Quarter.Core.UnitTest.Repositories;

public abstract class UserRepositoryTest : RepositoryTestBase<User, IUserRepository>
{
    protected override IdOf<User> ArbitraryId()
        => IdOf<User>.Random();

    protected override User ArbitraryAggregate()
    {
        var id = Guid.NewGuid();
        return new User(new Email($"{id}@example.com"), User.NoRoles);
    }

    protected override User WithoutTimestamps(User aggregate)
        => aggregate; // with { Created = null, Updated = null };

    protected override User Mutate(User aggregate)
    {
        aggregate.Email = new Email($"mutated_{aggregate.Email.AsString()}");
        return aggregate;
    }

    [Test]
    public void ThrowsWhenGettingUnknownUserByEmail()
    {
        var repository = Repository();

        Assert.ThrowsAsync<NotFoundException>(() => repository.GetUserByEmailAsync("bob.doe@example.com", CancellationToken.None));
    }

    [Test]
    public async Task ItShouldGetUserByEmail()
    {
        var repository = Repository();
        var id = IdOf<User>.Random();
        var user = new User(new Email($"{id.AsString()}@example.com"));
        await repository.CreateAsync(user, CancellationToken.None);
        var readUser = await repository.GetUserByEmailAsync($"{id.AsString()}@example.com", CancellationToken.None);

        Assert.That(readUser, Is.EqualTo(user));
    }

    [Test]
    public async Task ItShouldGetUserByEmailRegardlessOfCase()
    {
        var repository = Repository();
        var id = IdOf<User>.Random();
        var user = new User(new Email($"{id.AsString()}@example.com"));
        await repository.CreateAsync(user, CancellationToken.None);
        var readUser = await repository.GetUserByEmailAsync($"{id.AsString()}@EXAMPLE.COM", CancellationToken.None);

        Assert.That(readUser, Is.EqualTo(user));
    }

    [Test]
    public async Task ItShouldFailIfUserEmailIsInUse()
    {
        var repository = Repository();
        var userA = new User(new Email("jane.doe@example.com"));
        var userB = new User(new Email("jane.doe@example.com"));
        await repository.CreateAsync(userA, CancellationToken.None);

        var ex = Assert.CatchAsync<ArgumentException>(() => repository.CreateAsync(userB, CancellationToken.None));
        Assert.That(ex?.Message, Does.Contain("Could not store"));
    }

    [Test]
    public async Task ItStoreUserRole()
    {
        var repository = Repository();

        var user = User.AdminUser(new Email("hello@example.com"));
        await repository.CreateAsync(user, CancellationToken.None);

        var readUser = await repository.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.That(readUser.Roles, Does.Contain(UserRole.Administrator));
    }

    [Test]
    public async Task ThrowsIfUpdatingToEmailInUse()
    {
        var repository = Repository();
        var userA = new User(new Email("jane.doe@example.com"));
        await repository.CreateAsync(userA, CancellationToken.None);

        var userB = new User(new Email("john.doe@example.com"));
        userB = await repository.CreateAsync(userB, CancellationToken.None);
        userB.Email = userA.Email;

        var ex = Assert.CatchAsync<ArgumentException>(() => repository.UpdateByIdAsync(userB.Id, _ => userB, CancellationToken.None));

        Assert.That(ex?.Message, Does.Contain("Could not store"));
    }

    [Test]
    public async Task ItShouldNotHaveLastLoginForUserThatNeverLoggedIn()
    {
        var repository = Repository();
        var user = await repository.CreateAsync(ArbitraryAggregate(), CancellationToken.None);

        var lastLogins = await repository.GetLastLoginsAsync(CancellationToken.None);

        Assert.That(lastLogins.ContainsKey(user.Id), Is.False);
    }

    [Test]
    public async Task ItShouldRecordLastLogin()
    {
        var repository = Repository();
        var user = await repository.CreateAsync(ArbitraryAggregate(), CancellationToken.None);
        var timestamp = UtcDateTime.Now();

        await repository.RecordUserLoginAsync(user.Id, timestamp, CancellationToken.None);
        var lastLogins = await repository.GetLastLoginsAsync(CancellationToken.None);

        // Postgres stores microseconds.
        Assert.That(lastLogins[user.Id].DateTime, Is.EqualTo(timestamp.DateTime).Within(TimeSpan.FromMilliseconds(1)));
    }

    [Test]
    public async Task ItShouldKeepTheMostRecentLogin()
    {
        var repository = Repository();
        var user = await repository.CreateAsync(ArbitraryAggregate(), CancellationToken.None);
        var recent = UtcDateTime.Now();
        var older = new UtcDateTime(recent.DateTime.AddHours(-1));

        await repository.RecordUserLoginAsync(user.Id, recent, CancellationToken.None);
        await repository.RecordUserLoginAsync(user.Id, older, CancellationToken.None);
        var lastLogins = await repository.GetLastLoginsAsync(CancellationToken.None);

        Assert.That(lastLogins[user.Id].DateTime, Is.EqualTo(recent.DateTime).Within(TimeSpan.FromMilliseconds(1)));
    }

    [Test]
    public async Task ItShouldNotSaveLastLoginForAnUnknownUser()
    {
        var repository = Repository();
        var unknownUserId = IdOf<User>.Random();

        await repository.RecordUserLoginAsync(unknownUserId, UtcDateTime.Now(), CancellationToken.None);
        var lastLogins = await repository.GetLastLoginsAsync(CancellationToken.None);

        Assert.That(lastLogins.ContainsKey(unknownUserId), Is.False);
    }

    [Test]
    public async Task ItShouldNotChangeUpdatedTimestampWhenRecordingLogin()
    {
        var repository = Repository();
        var user = await repository.CreateAsync(ArbitraryAggregate(), CancellationToken.None);

        await repository.RecordUserLoginAsync(user.Id, UtcDateTime.Now(), CancellationToken.None);
        var readUser = await repository.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.That(readUser.Updated, Is.Null);
    }
}

[TestFixture]
public class InMemoryUserRepositoryTest : UserRepositoryTest
{
    protected override IUserRepository Repository()
        => new InMemoryUserRepository();
}

[TestFixture]
[Category(TestCategories.DatabaseDependency)]
public class PostgresUserRepositoryTest : UserRepositoryTest
{
    protected override IUserRepository Repository()
        => new PostgresUserRepository(UnitTestPostgresConnectionProvider.Instance);
}
