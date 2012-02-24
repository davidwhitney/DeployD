using System;
using System.Collections.Generic;
using System.IO;
using NuGet;
using log4net;

namespace Deployd.Core.Caching
{
    /// <summary>
    /// Doesn't work with a debugger attached, framework bug fixed in 4.5
    /// </summary>
    public class NuGetPackageCache : INuGetPackageCache
    {
        protected static readonly ILog Logger = LogManager.GetLogger("NuGetPackageCache"); 

        private readonly string _cacheDirectory;

        public NuGetPackageCache() : this("package_cache")
        {
        }

        public NuGetPackageCache(string cacheDirectory)
        {
            _cacheDirectory = cacheDirectory;

            EnsureDirectoryExists(_cacheDirectory);
        }

        public void Cache(IPackage package)
        {
            var packageCache = _cacheDirectory + "/" + package.Id;

            EnsureDirectoryExists(packageCache);

            var packagePath = packageCache + "/" + package.Version + ".zip";

            if (File.Exists(packagePath))
            {
                Logger.DebugFormat("Skpping caching '{0}', cached item already exists.", packagePath);
                return;
            }

            File.WriteAllBytes(packagePath, package.GetStream().ReadAllBytes());
            Logger.InfoFormat("Cached {0} to {1}.", package.Id, package);
        }

        public void Cache(IList<IPackage> allAvailablePackages)
        {
            foreach (var package in allAvailablePackages)
            {
                Cache(package);
            }
        }

        private void EnsureDirectoryExists(string cacheDirectory)
        {
            if (!Directory.Exists(cacheDirectory))
            {
                Logger.InfoFormat("Creating cache directory '{0}'.", cacheDirectory);
                Directory.CreateDirectory(cacheDirectory);
            }
        }
    }
}
