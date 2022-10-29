using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quarter.Core.Auth;
using Quarter.Core.Models;

namespace Quarter.UnitTest.TestUtils;

public class HttpSession
{
    public HttpClient HttpClient { get; }
    private TestServer HttpServer { get; }
    private readonly IList<Claim> _fakeUserClaims = new List<Claim>();

    public const string IntegrationTestFakeAuthenticationScheme = nameof(IntegrationTestFakeAuthenticationScheme);

    public HttpSession()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .AddEnvironmentVariables()
            .Build();

        var webBuilder = new WebHostBuilder();
        webBuilder.UseConfiguration(config);
        webBuilder.UseStartup(ctx => new Startup(ctx.Configuration));
        webBuilder.UseEnvironment("test");

        var startupAssemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
        webBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName);

        SetupFakeAuthentication(webBuilder);

        HttpServer = new TestServer(webBuilder);
        HttpClient = HttpServer.CreateClient();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(IntegrationTestFakeAuthenticationScheme);
    }

    public T ResolveService<T>() where T : class
    {
        var srv = HttpServer.Services.GetService(typeof(T)) as T;
        if (srv is null) throw new InvalidOperationException($"Could not resolve service of type {typeof(T)}");
        return srv;
    }

    public void ClearFakeUserClaims()
        => _fakeUserClaims.Clear();

    public void FakeUserSession(User user)
    {
        _fakeUserClaims.Add(new Claim(ApplicationClaim.QuarterUserIdClaimType, user.Id.Id.ToString()));
        _fakeUserClaims.Add(new Claim(ClaimTypes.Email, user.Email.Value));
        _fakeUserClaims.Add(new Claim(ClaimTypes.Name, "test@example.com"));
    }

    private void SetupFakeAuthentication(WebHostBuilder webHostBuilder)
    {
        webHostBuilder.ConfigureTestServices(services =>
        {
            // Inject the list of claims to make it available to FakeAuthenticationHandler
            services.AddSingleton<Func<Claim[]>>(() => _fakeUserClaims.ToArray());

            services
                .AddAuthentication(IntegrationTestFakeAuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(IntegrationTestFakeAuthenticationScheme, options => { });
        });
    }
}

public class FakeAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly Func<Claim[]> _fakeClaimResolver;

    public FakeAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        Func<Claim[]> fakeClaimResolver)
        : base(options, logger, encoder, clock)
    {
        _fakeClaimResolver = fakeClaimResolver;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Evaluate and get the claims setup in the HTTP session
        var claims = _fakeClaimResolver();
        if (!claims.Any()) Task.FromResult(AuthenticateResult.NoResult());

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "FakeAuthentication")),
            HttpSession.IntegrationTestFakeAuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}