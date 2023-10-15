using System;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Options;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
using Quarter.Core.UI.State;
using Quarter.HttpApi.Services;
using Quarter.Services;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest;

[TestFixture]
public class QuarterServiceConfigurationTest
{
    [TestCase(typeof(IRepositoryFactory))]
    [TestCase(typeof(ICommandHandler))]
    [TestCase(typeof(IQueryHandler))]
    [TestCase(typeof(IStateManager<ApplicationState>))]
    [TestCase(typeof(IUserAuthorizationService))]
    [TestCase(typeof(IApiService))]
    [TestCase(typeof(IOptions<AuthOptions>))]
    [TestCase(typeof(IAdminService))]
    public void HasRegisteredService(Type type)
    {
        var provider = CreateServiceProvider();
        Assert.That(provider.GetService(type), Is.Not.Null);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddScoped<AuthenticationStateProvider>(_ => new TestAuthenticationStateProvider());
        serviceCollection.UseQuarterUnderTest();
        return serviceCollection.BuildServiceProvider();
    }
}