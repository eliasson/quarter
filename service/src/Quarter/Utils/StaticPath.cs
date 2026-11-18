using System.IO;
using Microsoft.Extensions.Configuration;
using Quarter.Core.Options;

namespace Quarter.Utils;

public static class StaticPath
{
    public static string GetStaticPath(this IConfiguration configuration, string contentRootPath)
    {
        var options = configuration.GetSection("Storage");
        var relPath = options[nameof(StorageOptions.StaticPath)] ?? "";
        return Path.Combine(contentRootPath, relPath);
    }

    /// <summary>
    /// The directory holding the time tracking client application, served on /ui.
    /// </summary>
    public static string GetAppPath(this IConfiguration configuration, string contentRootPath)
        => Path.Combine(configuration.GetStaticPath(contentRootPath), "ui");

    /// <summary>
    /// The directory holding the admin client application, served on /admin.
    /// </summary>
    public static string GetAdminPath(this IConfiguration configuration, string contentRootPath)
        => Path.Combine(configuration.GetStaticPath(contentRootPath), "admin");
}
