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
        var provider = CreateServiceProvider();
        var startupTasks = provider.GetServices<IStartupTask>()
            .Select(t => t.GetType());
        Assert.That(startupTasks, Is.EquivalentTo(new []
        {
            typeof(InitialUserStartupTask)
        }));
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.UseQuarterUnderTest();
        serviceCollection.RegisterStartupTasks();
        return serviceCollection.BuildServiceProvider();
    }
}
