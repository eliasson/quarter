using Microsoft.Extensions.Options;
using NUnit.Framework;
using Quarter.Core.Options;
using Quarter.Core.Repositories;
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

    public abstract class TestCase
    {
        protected AdminService Service;
        private readonly InMemoryRepositoryFactory _repositoryFactory = new ();

        protected void SetupTestCase(bool openRegistration)
        {
            Service = new AdminService(
                Options.Create(new AuthOptions { OpenRegistration = openRegistration }),
                _repositoryFactory);
        }
    }
}