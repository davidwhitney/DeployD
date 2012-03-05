using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;
using log4net;
using IFileSystem = System.IO.Abstractions.IFileSystem;

namespace Deployd.Core.Caching
{
    /// <summary>
    /// Doesn't work with a debugger attached, framework bug fixed in 4.5
    /// </summary>
    public class NuGetPackageCache : INuGetPackageCache
    {
        private readonly IFileSystem _fileSystem;
        protected static readonly ILog Logger = LogManager.GetLogger("NuGetPackageCache"); 

        private readonly string _cacheDirectory;

        public NuGetPackageCache(IFileSystem fileSystem) : this(fileSystem, "package_cache")
        {
        }

        public NuGetPackageCache(IFileSystem fileSystem, string cacheDirectory)
        {
            if (fileSystem == null) throw new ArgumentNullException("fileSystem");
            if (string.IsNullOrWhiteSpace(cacheDirectory)) throw new ArgumentException("", "cacheDirectory");

            _fileSystem = fileSystem;
            _cacheDirectory = cacheDirectory;

            _fileSystem.EnsureDirectoryExists(_cacheDirectory);
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

            _fileSystem.EnsureDirectoryExists(packageCache);

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

        public IPackage GetLatestVersion(string packageId)
        {
            string location = PackageCacheLocation(packageId);
            var versions = AvailablePackageVersions(packageId);
            
            List<IPackage> foundPackages = new List<IPackage>();
            foreach(var version in versions)
            {
                var package = new ZipPackage(Path.Combine(PackageCacheLocation(packageId), version));
                foundPackages.Add(package);
            }

            return foundPackages.SingleOrDefault(p => p.IsLatestVersion);
        }

        public IPackage GetSpecificVersion(string packageId, string version)
        {
            if (!File.Exists(Path.Combine(PackageCacheLocation(packageId), version)))
            {
                throw new ArgumentOutOfRangeException("version");
            }

            return new ZipPackage(Path.Combine(PackageCacheLocation(packageId), version));
        }
    }
}
