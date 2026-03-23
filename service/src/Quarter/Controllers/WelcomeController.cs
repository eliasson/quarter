using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quarter.Utils;

namespace Quarter.Controllers;

public class WelcomeController(ILogger<WebAppController> logger) : Controller
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Index([FromServices] IWebHostEnvironment env, [FromServices] IConfiguration configuration)
    {
        logger.LogDebug("Serving the ui via index.html");

        var indexPath = Path.Combine(configuration.GetStaticPath(env.ContentRootPath), "welcome.html");
        return PhysicalFile(indexPath, "text/html");
    }
}

