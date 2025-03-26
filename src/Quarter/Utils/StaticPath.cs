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
}
