using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quarter.Auth;
using Quarter.Core.Exceptions;
using Quarter.Core.Options;
using Quarter.Services;
using Quarter.StartupTasks;
using Quarter.Utils;

namespace Quarter;

public class Startup(IConfiguration configuration)
{
    private IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<InitialUserOptions>(Configuration.GetSection("InitialUser"));
        services.Configure<StorageOptions>(Configuration.GetSection("Storage"));
        services.Configure<AuthOptions>(Configuration.GetSection("Auth"));
        services.UseQuarter();
        ConfigureAuth(services);

        services.AddRouting();
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddControllers(o => o.EnableEndpointRouting = false);
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.RegisterStartupTasks();
    }

    private void ConfigureAuth(IServiceCollection services)
    {
        // NOTE: The authentication wiring is manually tested!
        var localMode = Configuration.GetValue<bool>("LocalMode");
        if (localMode)
        {
            services.AddScoped<IUserAuthorizationService, LocalAuthorizationService>();
            services.AddAuthentication(nameof(LocalModeAuthenticationHandler))
                .AddScheme<LocalModeAuthenticationOptions, LocalModeAuthenticationHandler>(
                    nameof(LocalModeAuthenticationHandler),
                    _ => { });
            return;
        }

        services.AddAuthentication(options =>
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

            })
            .AddGoogle(options =>
            {
                options.ClientId = Configuration["Auth:Providers:Google:ClientId"] ?? "missing-config";
                options.ClientSecret = Configuration["Auth:Providers:Google:ClientSecret"] ?? "missing-config";
                options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = OnCreatingTicket()
                };
            })
            .AddGitHub(options =>
            {
                options.ClientId = Configuration["Auth:Providers:GitHub:ClientId"] ?? "missing-config";
                options.ClientSecret = Configuration["Auth:Providers:GitHub:ClientSecret"] ?? "missing-config";
                options.Scope.Add("user:email");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = OnCreatingTicket()
                };
            });
    }

    private static Func<OAuthCreatingTicketContext, Task> OnCreatingTicket()
    {
        return async context =>
        {
            var authService = context.HttpContext.RequestServices.GetService<IUserAuthorizationService>();
            if (authService is null) return;

            var emailClaim = context.Principal?.Claims.First(c => c.Type == ClaimTypes.Email);
            if (emailClaim is { })
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

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        // Use a separate pipeline for handling the error

        app.UseExceptionHandler(builder =>
        {
            builder.Use(async (HttpContext ctx, RequestDelegate dg) =>
            {
                var feature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // At least for integration-tests of controllers the outer exception is the UnauthorizedAccessException
                var exception = feature?.Error.InnerException ?? feature?.Error;

                string message = "An unhandled error occured";

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
                // await dg.Invoke();
            });
        });

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseHsts();
        app.UseHttpsRedirection();
        ConfigureStaticFiles(app, env, logger);
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMvc();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/app/{**segment}", "/Application/_ApplicationHost");
            endpoints.MapFallbackToPage("/admin/{**segment}", "/Admin/_AdminHost");

            // This route needs to be very last as it will serve any file requested from /app with the index.html
            // to enable HTML routing (e.g. reload /app/projects page in browser).
            endpoints.MapControllerRoute(
                name: "uiRoute",
                pattern: "ui/{*uiRoute}",
                defaults: new
                {
                    controller = "App",
                    action = "Index"
                });
        });
    }

    private void ConfigureStaticFiles(IApplicationBuilder app, IHostEnvironment env, ILogger logger)
    {
        var staticPath = Configuration.GetStaticPath(env.ContentRootPath);

        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(staticPath),
            RequestPath = "/ui",
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

        logger.LogInformation("Static path: {StaticPath}", staticPath);
    }
}
