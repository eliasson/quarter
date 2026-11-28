using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Quarter.Auth;
using Quarter.Core.Exceptions;
using Quarter.Core.Options;
using Quarter.Services;
using Quarter.StartupTasks;
using Quarter.Utils;

namespace Quarter;

public static class WebApplicationConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Register all services used by the Quarter web application.
        /// </summary>
        public WebApplicationBuilder AddQuarter()
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            services.Configure<InitialUserOptions>(configuration.GetSection("InitialUser"));
            services.Configure<StorageOptions>(configuration.GetSection("Storage"));
            services.Configure<AuthOptions>(configuration.GetSection("Auth"));
            services.UseQuarter();
            ConfigureAuth(services, configuration);

            services.AddControllers();
            services.AddAuthorization();
            services.AddHttpContextAccessor();
            var localMode = configuration.GetValue<bool>("LocalMode");
            services.RegisterStartupTasks(localMode);

            return builder;
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Configure the request pipeline and endpoints for the Quarter web application.
        /// </summary>
        public WebApplication UseQuarter()
        {
            // Use a separate pipeline for handling the error
            app.UseExceptionHandler(builder =>
            {
                builder.Use(async (HttpContext ctx, RequestDelegate _) =>
                {
                    var feature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    // At least for integration-tests of controllers the outer exception is the UnauthorizedAccessException
                    var exception = feature?.Error.InnerException ?? feature?.Error;

                    string message;

                    switch (exception)
                    {
                        case UnauthorizedAccessException uae:
                            message = uae.Message;
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            break;

                        case NotFoundException nfe:
                            message = nfe.Message;
                            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            break;

                        default:
                            message = "An unhandled error occured";
                            break;
                    }

                    await ctx.Response.WriteAsync(message);
                });
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseClientApplications();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<UserActivityMiddleware>();

            app.MapGet("/app/{*remainder}", ctx =>
            {
                ctx.Response.Redirect("/ui");
                return Task.CompletedTask;
            });

            app.MapControllers();

            // Serve the admin index.html for any request below /admin that is not a static file, to enable HTML
            // routing in the admin application (e.g. reload /admin/users in browser).
            app.MapControllerRoute(
                name: "adminRoute",
                pattern: "admin/{*adminRoute}",
                defaults: new
                {
                    controller = "AdminApp",
                    action = "Index"
                });

            // This route needs to be very last as it will serve any file requested from /app with the index.html
            // to enable HTML routing (e.g. reload /app/projects page in browser).
            app.MapControllerRoute(
                name: "uiRoute",
                pattern: "ui/{*uiRoute}",
                defaults: new
                {
                    controller = "WebApp",
                    action = "Index"
                });

            // The index controller that serves the "ui/welcome.html" as root page.
            app.MapControllerRoute(
                name: "welcomeRoute",
                pattern: "/",
                defaults: new
                {
                    controller = "Welcome",
                    action = "Index"
                });

            return app;
        }

        /// <summary>
        /// Apply all pending database migrations.
        /// </summary>
        public void RunMigrations()
        {
            using var scope = app.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        /// <summary>
        /// Execute all registered startup tasks, in registration order.
        /// </summary>
        public async Task RunStartupTasksAsync()
        {
            var startupTasks = app.Services.GetServices<IStartupTask>();
            foreach (var startupTask in startupTasks)
                await startupTask.ExecuteAsync();
        }

        private void UseClientApplications()
        {
            var contentRootPath = app.Environment.ContentRootPath;

            app.UseClientApplication(app.Configuration.GetAppPath(contentRootPath), "/ui", serveDefaultFile: true);

            // The admin index.html is served by the AdminAppController (and not as a default file) so that it is only
            // served to administrators.
            app.UseClientApplication(app.Configuration.GetAdminPath(contentRootPath), "/admin", serveDefaultFile: false);
        }

        private void UseClientApplication(string path, string requestPath, bool serveDefaultFile)
        {
            if (!Directory.Exists(path))
            {
                app.Logger.LogWarning("No client application found at {Path} for routes at {RequestPath}", path, requestPath);
                return;
            }

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = requestPath,
                EnableDefaultFiles = serveDefaultFile,
                StaticFileOptions =
                {
                    OnPrepareResponse = ctx =>
                    {
                        if (ctx.File.Name != "index.html") return;

                        // Do not cache the file since there is no cache bust when a new version is generated.
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
                        ctx.Context.Response.Headers.Append("Expires", "-1");
                        ctx.Context.Response.Headers.Append("X-Service", "quarter");
                    }
                }
            });

            app.Logger.LogInformation("Serving client application from {Path} on {RequestPath}", path, requestPath);
        }
    }

    private static void ConfigureAuth(IServiceCollection services, IConfiguration configuration)
    {
        // NOTE: The authentication wiring is manually tested!
        var localMode = configuration.GetValue<bool>("LocalMode");
        if (localMode)
        {
            services.AddScoped<IUserAuthorizationService, LocalAuthorizationService>();
            services.AddAuthentication(nameof(LocalModeAuthenticationHandler))
                .AddScheme<LocalModeAuthenticationOptions, LocalModeAuthenticationHandler>(
                    nameof(LocalModeAuthenticationHandler),
                    _ => { });
            return;
        }

        var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.SlidingExpiration = true;

                var handle = options.Events.OnRedirectToLogin;
                options.Events.OnRedirectToLogin = async (ctx) =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }
                    await handle(ctx);
                };

            });

        if (configuration.GetValue<string?>("Auth:Providers:Google:ClientId") is { Length: > 0 })
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = configuration["Auth:Providers:Google:ClientId"] ?? "missing-config";
                options.ClientSecret = configuration["Auth:Providers:Google:ClientSecret"] ?? "missing-config";
                options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = OnCreatingTicket()
                };
            });
        }


        if (configuration.GetValue<string?>("Auth:Providers:GitHub:ClientId") is { Length: > 0 })
        {
            authBuilder.AddGitHub(options =>
            {
                options.ClientId = configuration["Auth:Providers:GitHub:ClientId"] ?? "missing-config";
                options.ClientSecret = configuration["Auth:Providers:GitHub:ClientSecret"] ?? "missing-config";
                options.Scope.Add("user:email");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = OnCreatingTicket()
                };
            });

        }
    }

    private static Func<OAuthCreatingTicketContext, Task> OnCreatingTicket()
    {
        return async context =>
        {
            var authService = context.HttpContext.RequestServices.GetService<IUserAuthorizationService>();
            if (authService is null) return;

            var emailClaim = context.Principal?.Claims.First(c => c.Type == ClaimTypes.Email);
            if (emailClaim is not null)
            {
                var authResult = await authService.AuthorizeOrCreateUserAsync(emailClaim.Value, CancellationToken.None);
                if (authResult.State == AuthorizedState.Authorized)
                {
                    context.Principal?.AddIdentity(new ClaimsIdentity(authResult.Claims));
                    return;
                }
            }

            throw new UnauthorizedAccessException(
                $"User with email {emailClaim?.Value} does not have access to this service");
        };
    }
}
