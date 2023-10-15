using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Quarter.Core.Auth;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Options;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Services;

[TestFixture]
public class UserAuthorizationServiceTest
{
    [TestFixture]
    public class WhenThereIsUserSession : TestCase
    {
        private User _standardUser;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _standardUser = await AddUser("jane.doe@example.com");
            SetCurrentUser(_standardUser);
        }

        [Test]
        public async Task ItShouldReturnUserId()
        {
            var userId = await Service.CurrentUserId();
            Assert.That(userId, Is.EqualTo(_standardUser.Id));
        }
    }

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
            var result = await Service.AuthorizeOrCreateUserAsync(_standardUser!.Email.Value, default);

            Assert.That(result.State,Is.EqualTo(AuthorizedState.Authorized));
        }

        [Test]
        public async Task ItShouldHaveUserIdClaim()
        {
            var result = await Service.AuthorizeOrCreateUserAsync(_standardUser!.Email.Value, default);
            var claims = result.Claims.Select(c => (c.Type, c.Value));

            Assert.That(claims, Is.EquivalentTo(new []
            {
                (ApplicationClaim.QuarterUserIdClaimType, _standardUser.Id.AsString()),
            }));
        }

        [Test]
        public async Task ItShouldHaveAdminClaim()
        {
            var result = await Service.AuthorizeOrCreateUserAsync(_adminUser!.Email.Value, default);
            var claims = result.Claims.Select(c => (c.Type, c.Value));

            Assert.That(claims, Is.EquivalentTo(new []
            {
                (ApplicationClaim.QuarterUserIdClaimType, _adminUser.Id.AsString()),
                (ClaimTypes.Role, "administrator"),
            }));
        }
    }

    [TestFixture]
    public class WhenThereIsNoRegisteredUserAndRegistrationIsClosed : TestCase
    {
        [Test]
        public async Task ItShouldReturnUnauthenticated()
        {
            SetOpenRegistration(false);
            var result = await Service.AuthorizeOrCreateUserAsync("jane.doe@example.com", default);

            Assert.That(result.State, Is.EqualTo(AuthorizedState.NotAuthorized));
        }

        [Test]
        public void ItShouldNotCreateUser()
            => Assert.ThrowsAsync<NotFoundException>(() => GetUser("jane.doe@examepl.com"));
    }

    [TestFixture]
    public class WhenThereIsNoRegisteredUserAndRegistrationIsOpen : TestCase
    {
        private AuthorizedResult _result;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SetOpenRegistration(true);
            _result = await Service.AuthorizeOrCreateUserAsync("jane.doe@example.com", default);
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
    }

    public class TestCase
    {

        private readonly IRepositoryFactory _repositoryFactory = new InMemoryRepositoryFactory();
        protected readonly UserAuthorizationService Service;
        private readonly TestAuthenticationStateProvider _authenticationStateProvider;
        private readonly AuthOptions _authOptions = new () { OpenRegistration = false };

        protected TestCase()
        {
            _authenticationStateProvider= new TestAuthenticationStateProvider();
            Service = new UserAuthorizationService(_authenticationStateProvider,
                _repositoryFactory,
                NullLogger<UserAuthorizationService>.Instance,
                Options.Create(_authOptions));
        }

        protected void SetOpenRegistration(bool openRegistration)
            => _authOptions.OpenRegistration = openRegistration;

        protected void SetCurrentUser(User loggedInUser)
            => _authenticationStateProvider.SetCurrentUser(loggedInUser.Id);

        protected Task<User> AddUser(string email)
            => AddUser(User.StandardUser(new Email(email)));

        protected Task<User> AddAdminUser(string email)
            => AddUser(User.AdminUser(new Email(email)));

        protected Task<User> GetUser(string email)
            => _repositoryFactory.UserRepository().GetUserByEmailAsync(email, default);

        private async Task<User> AddUser(User user)
        {
            var userRepository = _repositoryFactory.UserRepository();
            await userRepository.CreateAsync(user, default);
            return user;
        }
    }
}