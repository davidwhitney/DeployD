using System.Collections.Generic;
using System.IO;
using Deployd.Core.AgentConfiguration;
using NuGet;
using IFileSystem = System.IO.Abstractions.IFileSystem;

namespace Deployd.Core.PackageCaching
{
    public class InstalledPackageArchive : IInstalledPackageArchive
    {
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly IFileSystem _fileSystem;
        private readonly ILocalPackageCache _packageCache;

        public InstalledPackageArchive(IAgentSettingsManager agentSettingsManager, IFileSystem fileSystem, ILocalPackageCache packageCache)
        {
            _agentSettingsManager = agentSettingsManager;
            _fileSystem = fileSystem;
            _packageCache = packageCache;
        }

        public IEnumerable<IPackage> GetCurrentInstalledPackages()
        {
            var latestFolder = new DirectoryInfo(_agentSettingsManager.Settings.LatestDirectory);
            
            if (latestFolder.Exists)
            {
                var packageFolders = latestFolder.GetDirectories();
                foreach(var folder in packageFolders)
                {
                    var installMarker = folder.GetFiles("installed.txt");
                    
                    if (installMarker.Length <= 0)
                    {
                        continue;
                    }

                    var versionString = _fileSystem.File.ReadAllText(installMarker[0].FullName);
                    var packagePath = Path.Combine(folder.FullName, string.Format("{0}-{1}.nupkg", folder.Name, versionString));

                    if (_fileSystem.File.Exists(packagePath))
                    {
                        yield return new ZipPackage(packagePath);
                    }
                }
            }
        }

        public IPackage GetCurrentInstalledVersion(string packageId)
        {
            var latestPackageLocation = Path.Combine(_agentSettingsManager.Settings.LatestDirectory, packageId);
            if (_fileSystem.Directory.Exists(latestPackageLocation))
            {
                var installMarkerPath = Path.Combine(latestPackageLocation, "installed.txt");
                if (_fileSystem.File.Exists(installMarkerPath))
                {
                    var versionString = _fileSystem.File.ReadAllText(installMarkerPath);
                    return _packageCache.GetSpecificVersion(packageId, versionString);
                }
            }

            return null;
        }

        public void SetCurrentInstalledVersion(IPackage package)
        {
            var latestPackageLocation = Path.Combine(_agentSettingsManager.Settings.LatestDirectory, package.Id);

            if (!_fileSystem.Directory.Exists(latestPackageLocation))
            {
                _fileSystem.Directory.CreateDirectory(latestPackageLocation);
            }

            var installMarkerPath = Path.Combine(latestPackageLocation, "installed.txt");
            _fileSystem.File.WriteAllText(installMarkerPath, package.Version.ToString());
        }
    }
}
