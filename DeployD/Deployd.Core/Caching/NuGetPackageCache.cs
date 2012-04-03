using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deployd.Core.AgentConfiguration;
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

        public NuGetPackageCache(IFileSystem fileSystem, IAgentSettings agentSettings)
            : this(fileSystem, agentSettings.CacheDirectory)
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
            var packageCache = PackageCacheLocation(package);

            _fileSystem.EnsureDirectoryExists(packageCache);

            var packagePath = Path.Combine(packageCache, CachedPackageVersionFilename(package.Id, package.Version.ToString()));
            
            Logger.DebugFormat("Evaluating packageId: '{0}' - ver '{1}', cached item already exists.", package.Id, package.Version);

            if (File.Exists(packagePath))
            {
                Logger.DebugFormat("Skpping caching '{0}', cached item already exists.", packagePath);
                return;
            }

            Logger.InfoFormat("Downloading {0} to {1}.", package.Id, package);
            File.WriteAllBytes(packagePath, package.GetStream().ReadAllBytes());
            Logger.InfoFormat("Cached {0} to {1}.", package.Id, package);
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

            return new ZipPackage(packagePath);
        }

        private static string CachedPackageVersionFilename(string packageId, string version)
        {
            return string.Format("{0}-{1}.nupkg", packageId, version);
        }
    }
}
