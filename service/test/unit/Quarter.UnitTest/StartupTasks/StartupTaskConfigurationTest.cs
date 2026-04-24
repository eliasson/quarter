using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Quarter.StartupTasks;

namespace Quarter.UnitTest.StartupTasks;

[TestFixture]
public class StartupTaskConfigurationTest
{
    [Test]
    public void ItShouldHaveRegisteredStartupTasks()
    {
        var provider = CreateServiceProvider(localMode: false);
        var startupTasks = provider.GetServices<IStartupTask>()
            .Select(t => t.GetType());
        Assert.That(startupTasks, Is.EquivalentTo(new[]
        {
            typeof(InitialUserStartupTask)
        }));
    }

    [Test]
    public void ItShouldRegisterLocalUserStartupTaskInLocalMode()
    {
        var provider = CreateServiceProvider(localMode: true);
        var startupTasks = provider.GetServices<IStartupTask>()
            .Select(t => t.GetType());
        Assert.That(startupTasks, Is.EquivalentTo(new[]
        {
            typeof(InitialUserStartupTask),
            typeof(LocalUserStartupTask)
        }));
    }

    private static ServiceProvider CreateServiceProvider(bool localMode)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.UseQuarterUnderTest();
        serviceCollection.RegisterStartupTasks(localMode);
        return serviceCollection.BuildServiceProvider();
    }
}
