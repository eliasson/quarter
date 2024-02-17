using Quarter.Core.UI.State;
using Quarter.State;
using Microsoft.Extensions.DependencyInjection;
using Quarter.Core.Commands;
using Quarter.Core.Migrations;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
using Quarter.HttpApi.Services;
using Quarter.Services;

namespace Quarter
{
    public static class QuarterServiceConfiguration
    {
        public static void UseQuarter(this IServiceCollection serviceCollection)
            => UseQuarter(serviceCollection, false);

        public static void UseQuarterUnderTest(this IServiceCollection serviceCollection)
            => UseQuarter(serviceCollection, true);

        private static void UseQuarter(this IServiceCollection serviceCollection, bool useInMemoryStorage)
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
            serviceCollection.AddScoped<IStateManager<ApplicationState>>(
                sp =>
                {
                    var repositoryFactory = sp.GetService<IRepositoryFactory>();
                    var commandHandler = sp.GetService<ICommandHandler>();
                    var authService = sp.GetService<IUserAuthorizationService>();

                    return new StateManager<ApplicationState>(
                        new ApplicationState(),
                        new ActionHandler(repositoryFactory!, commandHandler!, authService!));
                });
            serviceCollection.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
            serviceCollection.AddSingleton<IApiService, ApiService>();
            serviceCollection.AddSingleton<IAdminService, AdminService>();
        }
    }
}
