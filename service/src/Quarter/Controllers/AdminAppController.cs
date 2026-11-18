using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quarter.Core.Auth;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Utils;

namespace Quarter.Controllers;

/// <summary>
/// Controller used to serve the admin client application for any request below /admin. This enables the "HTML5"
/// mode, which is needed for the client routing to work (see <see cref="WebAppController"/>).
///
/// Only administrators are served the application. Note that this only hides the admin UI, the API is what enforces
/// access to the admin operations.
/// </summary>
public class AdminAppController(IRepositoryFactory repositoryFactory, ILogger<AdminAppController> logger) : Controller
{
    private const string LoginUrl = "/ui/login.html";

    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index([FromServices] IWebHostEnvironment env, [FromServices] IConfiguration configuration, CancellationToken ct)
    {
        var idClaim = User.FindFirst(ApplicationClaim.QuarterUserIdClaimType);
        if (idClaim is null || !Guid.TryParse(idClaim.Value, out var userId))
            return Redirect(LoginUrl);

        User user;
        try
        {
            user = await repositoryFactory.UserRepository().GetByIdAsync(IdOf<User>.Of(userId), ct);
        }
        catch (NotFoundException)
        {
            return Redirect(LoginUrl);
        }

        if (!user.IsAdmin())
        {
            logger.LogInformation("Denying user {UserId} access to the admin ui", userId);
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        logger.LogDebug("Serving the admin ui via index.html");

        var indexPath = Path.Combine(configuration.GetAdminPath(env.ContentRootPath), "index.html");
        return PhysicalFile(indexPath, "text/html");
    }
}
