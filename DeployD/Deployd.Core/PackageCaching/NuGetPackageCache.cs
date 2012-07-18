using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using NuGet;
using IFileSystem = System.IO.Abstractions.IFileSystem;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core.PackageCaching
{
    /// <summary>
    /// Doesn't work with a debugger attached, framework bug fixed in 4.5
    /// </summary>
    public class NuGetPackageCache : ILocalPackageCache
    {
        private readonly IFileSystem _fileSystem;
        protected readonly ILogger Logger;
        public event EventHandler<PackageEventArgs> OnUpdateStarted;
        public event EventHandler<PackageEventArgs> OnUpdateFinished;

        private readonly string _cacheDirectory;

        public NuGetPackageCache(IFileSystem fileSystem, IAgentSettingsManager agentSettings, ILogger logger)
            : this(fileSystem, agentSettings.Settings.CacheDirectory)
        {
            Logger = logger;
        }

        public NuGetPackageCache(IFileSystem fileSystem, string cacheDirectory)
        {
            if (fileSystem == null) throw new ArgumentNullException("fileSystem");
            if (string.IsNullOrWhiteSpace(cacheDirectory)) throw new ArgumentException("", "cacheDirectory");

            _fileSystem = fileSystem;
            _cacheDirectory = cacheDirectory;

            _fileSystem.EnsureDirectoryExists(_cacheDirectory);
        }

        public IEnumerable<IPackage> AllCachedPackages()
        {
            var cacheDir = new DirectoryInfo(_cacheDirectory);
            if (!cacheDir.Exists)
                cacheDir.Create();

            var subDirs = cacheDir.GetDirectories();
            foreach(var subDir in subDirs)
            {
                var packageFiles = subDir.GetFiles("*.nupkg");
                foreach(var packageFile in packageFiles)
                {
                    yield return new ZipPackage(packageFile.FullName);
                }
            }
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
            var versions = new List<string>();
            for (var index = 0; index < files.Count; index++)
            {
                var filename = Path.GetFileNameWithoutExtension(files[index]);
                if (filename.Contains("-"))
                {
                    var version = filename.Split(new[]{'-'}, StringSplitOptions.RemoveEmptyEntries).Last();
                    versions.Add(version);
                }
            }

            return versions.OrderByDescending(s=>s).ToList();
        } 

        public void Add(IPackage package)
        {
            var packageCacheLocation = PackageCacheLocation(package);

            _fileSystem.EnsureDirectoryExists(packageCacheLocation);

            var cachedPackagePath = Path.Combine(packageCacheLocation, CachedPackageVersionFilename(package.Id, package.Version.ToString()));
            
            if (CachedVersionExistsAndIsUpToDate(package, cachedPackagePath))
            {
                return;
            }

            Logger.Info("Downloading {0} to {1}.", package.Id, package);

            if (OnUpdateStarted != null)
            {
                OnUpdateStarted(this, new PackageEventArgs(package));
            }

            // save the package
            File.WriteAllBytes(cachedPackagePath, package.GetStream().ReadAllBytes());

            Logger.Info("Cached {0} to {1}.", package.Id, package);

            if (OnUpdateFinished != null)
            {
                OnUpdateFinished(this, new PackageEventArgs(package));
            }
        }

        private bool CachedVersionExistsAndIsUpToDate(IPackage package, string packagePath)
        {
            bool exists = File.Exists(packagePath);
            bool upToDate = !package.Published.HasValue
                             || package.Published.Value.LocalDateTime < File.GetLastWriteTime(packagePath);
            if (exists)
            {
                Logger.Debug("Evaluating packageId: '{0}' - ver '{1}', cached item already exists.", package.Id, package.Version);
            }
            if (upToDate)
            {
                Logger.Debug("Cached package is up to date");
            } else
            {
                Logger.Debug("Cached package needs updating");
            }
            return exists && upToDate;
        }

        private string PackageCacheLocation(IPackage package)
        {
            return PackageCacheLocation(package.Id);
        }

        private string PackageCacheLocation(string packageId)
        {
            return Path.Combine(_cacheDirectory, packageId);
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
            var versions = AvailablePackageVersions(packageId);
            
            var foundPackages = new List<IPackage>();
            foreach(var version in versions)
            {
                var packageFilename = CachedPackageVersionFilename(packageId, version);
                var packagePath = Path.Combine(PackageCacheLocation(packageId), packageFilename);
                
                if (File.Exists(packagePath))
                {
                    foundPackages.Add(new ZipPackage(packagePath));
                }
            }

            return foundPackages.OrderByDescending(p => p.Version).FirstOrDefault();
        }

        public IPackage GetSpecificVersion(string packageId, string version)
        {
            var filename = CachedPackageVersionFilename(packageId, version);
            var packagePath = Path.Combine(PackageCacheLocation(packageId), filename);
            if (!File.Exists(packagePath))
            {
                throw new ArgumentOutOfRangeException("version");
            }

            try
            {
                return new ZipPackage(packagePath);
            } catch (Exception ex)
            {
                return null;
            }
        }

        private static string CachedPackageVersionFilename(string packageId, string version)
        {
            return string.Format("{0}-{1}.nupkg", packageId, version);
        }
    }
}
