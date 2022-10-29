using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Quarter.UnitTest.TestUtils;

public class HttpSession
{
    public HttpClient HttpClient { get; }
    public TestServer HttpServer { get; }

    public HttpSession()
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

        HttpServer = new TestServer(webBuilder);
        HttpClient = HttpServer.CreateClient();
    }
}