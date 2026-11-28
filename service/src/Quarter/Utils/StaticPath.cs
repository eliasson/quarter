using System.IO;
using Microsoft.Extensions.Configuration;
using Quarter.Core.Options;

namespace Quarter.Utils;

public static class StaticPath
{
    extension(IConfiguration configuration)
    {
        /// <summary>
        /// The directory holding the time tracking client application, served on /ui.
        /// </summary>
        public string GetAppPath(string contentRootPath)
            => Path.Combine(configuration.GetStaticPath(contentRootPath), "ui");

        /// <summary>
        /// The directory holding the admin client application, served on /admin.
        /// </summary>
        public string GetAdminPath(string contentRootPath)
            => Path.Combine(configuration.GetStaticPath(contentRootPath), "admin");

        private string GetStaticPath(string contentRootPath)
        {
            var options = configuration.GetSection("Storage");
            var relPath = options[nameof(StorageOptions.StaticPath)] ?? "";
            return Path.Combine(contentRootPath, relPath);
        }
    }
}
