using System;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quarter.Core.Migrations;
using Quarter.Core.Repositories;
using Quarter.StartupTasks;

namespace Quarter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        RunMigrations(host);
        TryRunPostgresMigrations(host);
        await RunStartupTasks(host);

        await host.RunAsync();
    }

    private static void RunMigrations(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    private static void TryRunPostgresMigrations(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var postgresProvider = scope.ServiceProvider.GetService<IPostgresConnectionProvider>();
        if (postgresProvider is null) return;

        try
        {
            var services = new ServiceCollection();
            services.AddSingleton<IPostgresConnectionProvider>(postgresProvider);
            services.ConfigurePostgresMigrations();
            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IMigrationRunner>().MigrateUp();
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Could not run PostgreSQL migrations (PostgreSQL may not be available)");
        }
    }

    private static async Task RunStartupTasks(IHost host)
    {
        var startupTasks = host.Services.GetServices<IStartupTask>();
        foreach (var startupTask in startupTasks)
            await startupTask.ExecuteAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
