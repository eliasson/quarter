using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Options;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.Services;

namespace Quarter.UnitTest.Services;

[TestFixture]
public class AdminServiceTest
{
    [TestFixture]
    public class WhenUserRegistrationIsNotConfigured : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
            => SetupTestCase(openRegistration: false);

        [Test]
        public void ItShouldBeDisabled()
            => Assert.That(Service.IsUserRegistrationOpen(), Is.False);
    }

    [TestFixture]
    public class WhenUserRegistrationIsConfiguredToBeOpen : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
           => SetupTestCase(openRegistration: true);

        [Test]
        public void ItShouldBeEnabled()
            => Assert.That(Service.IsUserRegistrationOpen(), Is.True);
    }

    [TestFixture]
    public class WhenDataExists : TestCase
    {
        private SystemMetrics _metrics;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SetupTestCase();
            await AddUsersAsync(12);
            _metrics = await Service.GetSystemMetricsAsync(CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnTotalNumberOfUsersInSystem()
            => Assert.That(_metrics.NumberOfUsers, Is.EqualTo(12));
    }

    public abstract class TestCase
    {
        protected AdminService Service;
        private readonly InMemoryRepositoryFactory _repositoryFactory = new();

        protected void SetupTestCase(bool openRegistration = false)
        {
            Service = new AdminService(
                Options.Create(new AuthOptions { OpenUserRegistration = openRegistration }),
                _repositoryFactory);
        }

        protected async Task AddUsersAsync(uint numberOfUsers)
        {
            for (var i = 0; i < numberOfUsers; i++)
            {
                var user = new User(new Email($"{i}@example.com"), ArraySegment<UserRole>.Empty);
                await _repositoryFactory.UserRepository().CreateAsync(user, CancellationToken.None);
            }
        }
    }
}
