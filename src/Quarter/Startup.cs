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
using Quarter.Auth;
using Quarter.Core.Options;
using Quarter.Services;
using Quarter.StartupTasks;

namespace Quarter
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: Should these be moved to UserQuarter?
            services.Configure<InitialUserOptions>(Configuration.GetSection("InitialUser"));
            services.Configure<StorageOptions>(Configuration.GetSection("Storage"));
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
                    options.ClientId = Configuration["Auth:Providers:Google:ClientId"];
                    options.ClientSecret = Configuration["Auth:Providers:Google:ClientSecret"];
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
                    options.ClientId = Configuration["Auth:Providers:GitHub:ClientId"];
                    options.ClientSecret = Configuration["Auth:Providers:GitHub:ClientSecret"];
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
                    var authResult = await authService.IsUserAuthorized(emailClaim.Value, CancellationToken.None);
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use a separate pipeline for handling the error

            app.UseExceptionHandler(builder =>
            {
                builder.Use(async (HttpContext ctx, RequestDelegate dg) =>
                {
                    var feature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                    ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    var exception = feature?.Error?.InnerException;

                    string message = "An unhandled error occured";

                    switch(exception)
                    {
                        case UnauthorizedAccessException uae:
                            message = uae.Message;
                            ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
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
                endpoints.MapFallbackToPage("/app/{**segment}","/Application/_ApplicationHost");
                endpoints.MapFallbackToPage("/admin/{**segment}","/Admin/_AdminHost");
            });
        }
    }
}
