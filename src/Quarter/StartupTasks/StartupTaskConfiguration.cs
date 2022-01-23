using Microsoft.Extensions.DependencyInjection;

namespace Quarter.StartupTasks;

public static class StartupTaskConfiguration
{
    public static void RegisterStartupTasks(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IStartupTask, InitialUserStartupTask>();
    }
}