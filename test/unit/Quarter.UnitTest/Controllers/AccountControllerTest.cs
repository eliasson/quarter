using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Controllers
{
    [TestFixture]
    public class AccountControllerTest
    {
        public class WhenLoggingInUsingGitHub : AccountControllerTest
        {
            private HttpResponseMessage _response;

            [OneTimeSetUp]
            public async Task Setup()
                => _response = await LoginAsync("github");

            [Test]
            public void ItShouldRedirectToGitHubServer()
            {
                Assert.Multiple((() =>
                {
                    Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
                    Assert.That(_response.Headers.Location?.ToString(), Does.StartWith("https://github.com/login/oauth/authorize"));
                }));
            }
        }

        public class WhenLoggingInUsingGoogle : AccountControllerTest
        {
            private HttpResponseMessage _response;

            [OneTimeSetUp]
            public async Task Setup()
                => _response = await LoginAsync("google");

            [Test]
            public void ItShouldRedirectToGoogleServer()
            {
                Assert.Multiple((() =>
                {
                    Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
                    Assert.That(_response.Headers.Location?.ToString(), Does.StartWith("https://accounts.google.com/o/oauth2/v2/auth"));
                }));
            }
        }

        public class WhenLoggingInUsingUnknownProvider : AccountControllerTest
        {
            private HttpResponseMessage _response;

            [OneTimeSetUp]
            public async Task Setup()
                => _response = await LoginAsync("nasa");

            [Test]
            public void ItShouldResultInNotFoundError()
            {
                Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            }
        }

        private Task<HttpResponseMessage> LoginAsync(string provider) =>
            HttpTestClient.HttpClient.GetAsync($"/account/login/{provider}");
    }
}