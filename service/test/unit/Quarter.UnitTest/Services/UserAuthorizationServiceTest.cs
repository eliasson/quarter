using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Quarter.Core.Auth;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Options;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.Services;

namespace Quarter.UnitTest.Services;

[TestFixture]
public class UserAuthorizationServiceTest
{
    [TestFixture]
    public class WithLocalUsers : TestCase
    {
        private User _standardUser;
        private User _adminUser;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _standardUser = await AddUser("jane.doe@example.com");
            _adminUser = await AddAdminUser("admin@example.com");
        }

        [Test]
        public async Task ItShouldReturnAuthorizedResult()
        {
            var result = await Service.AuthorizeOrCreateUserAsync(_standardUser!.Email.Value, CancellationToken.None);

            Assert.That(result.State, Is.EqualTo(AuthorizedState.Authorized));
        }

        [Test]
        public async Task ItShouldRecordLastLogin()
        {
            _ = await Service.AuthorizeOrCreateUserAsync(_standardUser!.Email.Value, CancellationToken.None);

            Assert.That(await HasLastLogin(_standardUser.Email.Value), Is.True);
        }

        [Test]
        public async Task ItShouldHaveUserIdClaim()
        {
            var result = await Service.AuthorizeOrCreateUserAsync(_standardUser!.Email.Value, CancellationToken.None);
            var claims = result.Claims.Select(c => (c.Type, c.Value));

            Assert.That(claims, Is.EquivalentTo([
                (ApplicationClaim.QuarterUserIdClaimType, _standardUser.Id.AsString()),
            ]));
        }

        [Test]
        public async Task ItShouldHaveAdminClaim()
        {
            var result = await Service.AuthorizeOrCreateUserAsync(_adminUser!.Email.Value, CancellationToken.None);
            var claims = result.Claims.Select(c => (c.Type, c.Value));

            Assert.That(claims, Is.EquivalentTo([
                (ApplicationClaim.QuarterUserIdClaimType, _adminUser.Id.AsString()),
                (ClaimTypes.Role, "administrator"),
            ]));
        }
    }

    [TestFixture]
    public class WhenThereIsNoRegisteredUserAndRegistrationIsClosed : TestCase
    {
        [Test]
        public async Task ItShouldReturnUnauthenticated()
        {
            SetOpenRegistration(false);
            var result = await Service.AuthorizeOrCreateUserAsync("jane.doe@example.com", CancellationToken.None);

            Assert.That(result.State, Is.EqualTo(AuthorizedState.NotAuthorized));
        }

        [Test]
        public async Task ItShouldNotCreateUser()
            => await Assert.ThrowsAsync<NotFoundException>(() => GetUser("jane.doe@examepl.com"));
    }

    [TestFixture]
    public class WhenThereIsNoRegisteredUserAndRegistrationIsOpen : TestCase
    {
        private AuthorizedResult _result;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SetOpenRegistration(true);
            _result = await Service.AuthorizeOrCreateUserAsync("jane.doe@example.com", CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnAuthenticatedResult()
            => Assert.That(_result.State, Is.EqualTo(AuthorizedState.Authorized));

        [Test]
        public async Task ItShouldCreateUser()
        {
            var user = await GetUser("jane.doe@example.com");
            Assert.That(user, Is.Not.Null);
        }

        [Test]
        public async Task ItShouldRecordLastLoginForCreatedUser()
            => Assert.That(await HasLastLogin("jane.doe@example.com"), Is.True);
    }

    public class TestCase
    {
        private readonly IRepositoryFactory _repositoryFactory = new InMemoryRepositoryFactory();
        protected readonly UserAuthorizationService Service;
        private readonly AuthOptions _authOptions = new() { OpenUserRegistration = false };

        protected TestCase()
        {
            var commandHandler = new CommandHandler(_repositoryFactory);
            var loginTracker = new LoginTracker(_repositoryFactory, TimeProvider.System, NullLogger<LoginTracker>.Instance);

            Service = new UserAuthorizationService(_repositoryFactory,
                commandHandler,
                loginTracker,
                Options.Create(_authOptions),
                NullLogger<UserAuthorizationService>.Instance);
        }

        protected void SetOpenRegistration(bool openRegistration)
            => _authOptions.OpenUserRegistration = openRegistration;

        protected Task<User> AddUser(string email)
            => AddUser(User.StandardUser(new Email(email)));

        protected Task<User> AddAdminUser(string email)
            => AddUser(User.AdminUser(new Email(email)));

        protected async Task<bool> HasLastLogin(string email)
        {
            var user = await GetUser(email);
            var lastLogins = await _repositoryFactory.UserRepository().GetLastLoginsAsync(CancellationToken.None);
            return lastLogins.ContainsKey(user.Id);
        }

        protected Task<User> GetUser(string email)
            => _repositoryFactory.UserRepository().GetUserByEmailAsync(email, CancellationToken.None);

        private async Task<User> AddUser(User user)
        {
            var userRepository = _repositoryFactory.UserRepository();
            await userRepository.CreateAsync(user, CancellationToken.None);
            return user;
        }
    }
}
