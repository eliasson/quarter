using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Quarter.Auth;
using Quarter.Core.Auth;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Services;

[TestFixture]
public class UserAuthorizationServiceTest
{
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
            var result = await Service.IsUserAuthorized(_standardUser!.Email.Value, default);

            Assert.That(result.State,Is.EqualTo(AuthorizedState.Authorized));
        }

        [Test]
        public async Task ItShouldHaveUserIdClaim()
        {
            var result = await Service.IsUserAuthorized(_standardUser!.Email.Value, default);
            var claims = result.Claims.Select(c => (c.Type, c.Value));

            Assert.That(claims, Is.EquivalentTo(new []
            {
                (ApplicationClaim.QuarterUserIdClaimType, _standardUser.Id.AsString()),
            }));
        }

        [Test]
        public async Task ItShouldHaveAdminClaim()
        {
            var result = await Service.IsUserAuthorized(_adminUser!.Email.Value, default);
            var claims = result.Claims.Select(c => (c.Type, c.Value));

            Assert.That(claims, Is.EquivalentTo(new []
            {
                (ApplicationClaim.QuarterUserIdClaimType, _adminUser.Id.AsString()),
                (ClaimTypes.Role, "administrator"),
            }));
        }
    }

    public class WhenThereIsNoUser : TestCase
    {
        [Test]
        public async Task ItShouldReturnUnauthenticated()
        {
            var result = await Service.IsUserAuthorized("jane.doe@example.com", default);

            Assert.That(result.State, Is.EqualTo(AuthorizedState.NotAuthorized));
        }
    }

    public class TestCase
    {

        private readonly IRepositoryFactory _repositoryFactory = new InMemoryRepositoryFactory();
        protected readonly UserAuthorizationService Service;
        private readonly TestAuthenticationStateProvider _authenticationStateProvider;

        protected TestCase()
        {
            _authenticationStateProvider= new TestAuthenticationStateProvider();
            Service = new UserAuthorizationService(_authenticationStateProvider, _repositoryFactory, NullLogger<UserAuthorizationService>.Instance);
        }

        protected void SetCurrentUser(User loggedInUser)
            => _authenticationStateProvider.SetCurrentUser(loggedInUser.Id);

        protected Task<User> AddUser(string email)
            => AddUser(User.StandardUser(new Email(email)));

        protected Task<User> AddAdminUser(string email)
            => AddUser(User.AdminUser(new Email(email)));

        private async Task<User> AddUser(User user)
        {
            var userRepository = _repositoryFactory.UserRepository();
            await userRepository.CreateAsync(user, default);
            return user;
        }
    }
}