using System;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using NUnit.Framework;
using Quarter.Core.UnitTest.TestUtils;

namespace Quarter.Core.UnitTest.Repositories
{
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

            Assert.ThrowsAsync<NotFoundException>(() => repository.GetUserByEmailAsync("bob.doe@example.com", default));
        }

        [Test]
        public async Task ItShouldGetUserByEmail()
        {
            var repository = Repository();
            var id = IdOf<User>.Random();
            var user = new User(new Email($"{id.AsString()}@example.com"));
            await repository.CreateAsync(user, default);
            var readUser = await repository.GetUserByEmailAsync($"{id.AsString()}@example.com", default);

            Assert.That(readUser, Is.EqualTo(user));
        }

        [Test]
        public async Task ItShouldGetUserByEmailRegardlessOfCase()
        {
            var repository = Repository();
            var id = IdOf<User>.Random();
            var user = new User(new Email($"{id.AsString()}@example.com"));
            await repository.CreateAsync(user, default);
            var readUser = await repository.GetUserByEmailAsync($"{id.AsString()}@EXAMPLE.COM", default);

            Assert.That(readUser, Is.EqualTo(user));
        }

        [Test]
        public async Task ItShouldFailIfUserEmailIsInUse()
        {
            var repository = Repository();
            var userA = new User(new Email("jane.doe@example.com"));
            var userB = new User(new Email("jane.doe@example.com"));
            await repository.CreateAsync(userA, default);

            var ex = Assert.CatchAsync<ArgumentException>(() =>repository.CreateAsync(userB, default));
            Assert.That(ex?.Message, Does.Contain("Could not store"));
        }

        [Test]
        public async Task ItStoreUserRole()
        {
            var repository = Repository();

            var user = User.AdminUser(new Email("hello@example.com"));
            await repository.CreateAsync(user, default);

            var readUser = await repository.GetByIdAsync(user.Id, default);

            Assert.That(readUser.Roles, Does.Contain(UserRole.Administrator));
        }

        [Test]
        public async Task ThrowsIfUpdatingToEmailInUse()
        {
            var repository = Repository();
            var userA = new User(new Email("jane.doe@example.com"));
            await repository.CreateAsync(userA, default);

            var userB = new User(new Email("john.doe@example.com"));
            userB = await repository.CreateAsync(userB, default);
            userB.Email = userA.Email;

            var ex = Assert.CatchAsync<ArgumentException>(() => repository.UpdateByIdAsync(userB.Id, _ => userB, CancellationToken.None));

            Assert.That(ex?.Message, Does.Contain("Could not store"));
        }
    }

    public class InMemoryUserRepositoryTest : UserRepositoryTest
    {
        protected override IUserRepository Repository()
            => new InMemoryUserRepository();
    }

    [Category(TestCategories.DatabaseDependency)]
    public class PostgresUserRepositoryTest : UserRepositoryTest
    {
        protected override IUserRepository Repository()
            => new PostgresUserRepository(UnitTestPostgresConnectionProvider.Instance);
    }
}