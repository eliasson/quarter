using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Quarter.UnitTest.TestUtils
{
    public static class HttpTestClient
    {
        public static HttpClient HttpClient
            => _httpClient.Value;

        private static readonly Lazy<HttpClient> _httpClient = CreateClient();

        private static Lazy<HttpClient> CreateClient()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            var webBuilder = new WebHostBuilder();
            webBuilder.UseConfiguration(config);
            webBuilder.UseStartup(ctx => new Startup(ctx.Configuration));
            webBuilder.UseEnvironment("Development");

            var startupAssemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            webBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName);

            var server = new TestServer(webBuilder);
            return new Lazy<HttpClient>(() => server.CreateClient());
        }
    }
}