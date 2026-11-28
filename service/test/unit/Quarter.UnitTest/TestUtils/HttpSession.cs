using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quarter.Core.Auth;
using Quarter.Core.Models;

namespace Quarter.UnitTest.TestUtils;

public sealed class HttpSession : IDisposable
{
    public HttpClient HttpClient { get; }
    private readonly QuarterApplicationFactory _factory;
    private readonly List<Claim> _fakeUserClaims = [];

    public const string IntegrationTestFakeAuthenticationScheme = nameof(IntegrationTestFakeAuthenticationScheme);

    public HttpSession()
    {
        _factory = new QuarterApplicationFactory(SetupFakeAuthentication);
        HttpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Match the plain TestServer client the tests were written against.
            AllowAutoRedirect = false,
            HandleCookies = false,
        });
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(IntegrationTestFakeAuthenticationScheme);
    }

    public T ResolveService<T>() where T : class
    {
        var srv = _factory.Services.GetService(typeof(T)) as T;
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

    public void Dispose()
    {
        HttpClient.Dispose();
        _factory.Dispose();
    }

    private void SetupFakeAuthentication(IServiceCollection services)
    {
        // Override the ordinary authentication setup in the application with one that returns the injected
        // list of claims for a fake user.
        services.AddSingleton<Func<Claim[]>>(() => _fakeUserClaims.ToArray());
        services.AddAuthentication(options =>
        {
            var schemeBuilder = new AuthenticationSchemeBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
            {
                HandlerType = typeof(FakeAuthenticationHandler)
            };

            options.Schemes
                .First(s => s.Name == CookieAuthenticationDefaults.AuthenticationScheme)
                .HandlerType = typeof(FakeAuthenticationHandler);

            options.SchemeMap[CookieAuthenticationDefaults.AuthenticationScheme] = schemeBuilder;
        });
    }

    private sealed class QuarterApplicationFactory(Action<IServiceCollection> configureTestServices)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Run from the test output directory, where appsettings.test.json and TestData/static are located.
            builder.UseContentRoot(AppContext.BaseDirectory);
            builder.UseEnvironment("test");
            builder.ConfigureTestServices(configureTestServices);
        }
    }
}

public class FakeAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    Func<Claim[]> fakeClaimResolver)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Evaluate and get the claims setup in the HTTP session
        var claims = fakeClaimResolver();
        if (claims.Length == 0) return Task.FromResult(AuthenticateResult.NoResult());

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "FakeAuthentication")),
            HttpSession.IntegrationTestFakeAuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
