using Microsoft.Extensions.Options;
using NUnit.Framework;
using Quarter.Core.Options;
using Quarter.Services;

namespace Quarter.UnitTest.Services;

[TestFixture]
public class AdminServiceTest
{
    [TestFixture]
    public class WhenUserRegistrationIsNotConfigured
    {
        private AdminService _service;

        [OneTimeSetUp]
        public void Setup()
        {
            _service = new AdminService(Options.Create(new AuthOptions()));
        }

        [Test]
        public void ItShouldBeDisabled()
            => Assert.That(_service.IsUserRegistrationOpen(), Is.False);
    }

    [TestFixture]
    public class WhenUserRegistrationIsConfiguredToBeOpen
    {
        private AdminService _service;

        [OneTimeSetUp]
        public void Setup()
        {
            _service = new AdminService(Options.Create(new AuthOptions { OpenRegistration = true }));
        }

        [Test]
        public void ItShouldBeEnabled()
            => Assert.That(_service.IsUserRegistrationOpen(), Is.True);
    }
}