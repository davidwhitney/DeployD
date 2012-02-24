﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public IList<string> AvailablePackages
        {
            get
            {
                var dirs = Directory.GetDirectories(_cacheDirectory).ToList();

                for (var index = 0; index < dirs.Count; index++)
                {
                    dirs[index] = dirs[index].Replace(_cacheDirectory + "\\", "");
                }

                return dirs;
            }
        } 

        public IList<string> AvailablePackageVersions(string packageId)
        {
            var files = Directory.GetFiles(PackageCacheLocation(packageId)).ToList();

            for (var index = 0; index < files.Count; index++)
            {
                files[index] = files[index].Replace(_cacheDirectory + "/", "");
                files[index] = files[index].Replace(packageId + "\\", "");
            }

            return files;
        } 

        public void Add(IPackage package)
        {
            var packageCache = PackageCacheLocation(package);

            EnsureDirectoryExists(packageCache);

            var packagePath = packageCache + "/" + package.Id + "-" + package.Version + ".nupkg";

            if (File.Exists(packagePath))
            {
                Logger.DebugFormat("Skpping caching '{0}', cached item already exists.", packagePath);
                return;
            }

            File.WriteAllBytes(packagePath, package.GetStream().ReadAllBytes());
            Logger.InfoFormat("Cached {0} to {1}.", package.Id, package);
        }

        private string PackageCacheLocation(IPackage package)
        {
            return PackageCacheLocation(package.Id);
        }

        private string PackageCacheLocation(string packageId)
        {
            return _cacheDirectory + "/" + packageId;
        }

        public void Add(IEnumerable<IPackage> allAvailablePackages)
        {
            foreach (var package in allAvailablePackages)
            {
                Add(package);
            }
        }

        private static void EnsureDirectoryExists(string cacheDirectory)
        {
            if (Directory.Exists(cacheDirectory)) return;
            
            Logger.InfoFormat("Creating cache directory '{0}'.", cacheDirectory);
            Directory.CreateDirectory(cacheDirectory);
        }
    }
}
