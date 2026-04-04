using Microsoft.Extensions.DependencyInjection;

namespace Quarter.StartupTasks;

public static class StartupTaskConfiguration
{
    public static void RegisterStartupTasks(this IServiceCollection serviceCollection, bool localMode)
    {
        serviceCollection.AddTransient<IStartupTask, InitialUserStartupTask>();

        if (localMode)
            serviceCollection.AddTransient<IStartupTask, LocalUserStartupTask>();
    }
}
