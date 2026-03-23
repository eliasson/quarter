using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Controllers;

[TestFixture]
public class AppRedirectTest : HttpTestCase
{
    [TestFixture]
    public class WhenAccessingTheAppUrl : AppRedirectTest
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _response = await GetAsync();
        }

        [Test]
        public void ItShouldRedirectToUi()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
                Assert.That(_response.Headers.Location?.ToString(), Is.EqualTo("/ui"));
            });
        }
    }

    private Task<HttpResponseMessage> GetAsync() =>
        HttpTestSession.HttpClient.GetAsync("/app");
}
