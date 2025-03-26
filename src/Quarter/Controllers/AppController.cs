using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quarter.Utils;

namespace Quarter.Controllers;

/// <summary>
/// Controller used to serve the client side Vue app for any request. This enables the "HTML5" mode, which is needed
/// for the client routing to work.
///
/// E.g. when refreshing or deep linking to https://quarterapp.com/ui/projects we always sever the index page.
///
/// This works since this is added last in the list of endpoints, ie. the static file serving the CSS and JS will
/// take precedence.
/// </summary>
public class AppController(ILogger<AppController> logger) : Controller
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Index([FromServices] IWebHostEnvironment env, [FromServices] IConfiguration configuration)
    {
        logger.LogDebug("Serving the ui via index.html");

        var indexPath = Path.Combine(configuration.GetStaticPath(env.ContentRootPath), "index.html");
        return PhysicalFile(indexPath, "text/html");
    }
}
