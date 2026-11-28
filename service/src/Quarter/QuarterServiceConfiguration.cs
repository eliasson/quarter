using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quarter.Core.Commands;
using Quarter.Core.Migrations;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
using Quarter.HttpApi.Services;
using Quarter.Services;

namespace Quarter;

public static class QuarterServiceConfiguration
{
    extension(IServiceCollection serviceCollection)
    {
        public void UseQuarter()
            => serviceCollection.UseQuarter(false);

        public void UseQuarterUnderTest()
            => serviceCollection.UseQuarter(true);

        private void UseQuarter(bool useInMemoryStorage)
        {
            serviceCollection.AddSingleton<IPostgresConnectionProvider, DefaultPostgresConnectionProvider>();

            if (useInMemoryStorage)
            {
                serviceCollection.AddSingleton<IRepositoryFactory, InMemoryRepositoryFactory>();
            }
            else
            {
                serviceCollection.ConfigureMigrations();
                serviceCollection.AddSingleton<IRepositoryFactory, PostgresRepositoryFactory>();
            }

            serviceCollection.AddSingleton<ICommandHandler, CommandHandler>();
            serviceCollection.AddSingleton<IQueryHandler, QueryHandler>();
            serviceCollection.TryAddSingleton(TimeProvider.System);
            serviceCollection.AddSingleton<ILoginTracker, LoginTracker>();
            serviceCollection.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
            serviceCollection.AddSingleton<IApiService, ApiService>();
            serviceCollection.AddSingleton<IAdminService, AdminService>();
        }
    }
}
