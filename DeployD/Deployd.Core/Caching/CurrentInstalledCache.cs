using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using NuGet;

namespace Deployd.Core.Caching
{
    public class CurrentInstalledCache : ICurrentInstalledCache
    {
        private readonly IAgentSettings _agentSettings;
        private readonly INuGetPackageCache _packageCache;

        public CurrentInstalledCache(IAgentSettings agentSettings, INuGetPackageCache packageCache)
        {
            _agentSettings = agentSettings;
            _packageCache = packageCache;
        }

        public IEnumerable<IPackage> GetCurrentInstalledPackages()
        {
            string latestPackageLocation = Path.Combine(_agentSettings.LatestDirectory);
            DirectoryInfo latestFolder = new DirectoryInfo(latestPackageLocation);
            if (latestFolder.Exists)
            {
                var packageFolders = latestFolder.GetDirectories();
                foreach(var folder in packageFolders)
                {
                    var installMarker = folder.GetFiles("installed.txt");
                    if (installMarker.Length > 0)
                    {
                        string versionString = File.ReadAllText(installMarker[0].FullName);
                        string packagePath = Path.Combine(folder.FullName, string.Format("{0}-{1}.nupkg", folder.Name, versionString));
                        if (File.Exists(packagePath))
                        {
                            yield return new ZipPackage(packagePath);
                        }

                    }
                }
            }
        }

        public IPackage GetCurrentInstalledVersion(string packageId)
        {
            string latestPackageLocation = Path.Combine(_agentSettings.LatestDirectory, packageId);
            if (Directory.Exists(latestPackageLocation))
            {
                string installMarkerPath = Path.Combine(latestPackageLocation, "installed.txt");
                if (File.Exists(installMarkerPath))
                {
                    string versionString = File.ReadAllText(installMarkerPath);
                    return _packageCache.GetSpecificVersion(packageId, versionString);
                }
            }

            return null;
        }

        public void SetCurrentInstalledVersion(IPackage package)
        {
            string latestPackageLocation = Path.Combine(_agentSettings.LatestDirectory, package.Id);
            
            if (!Directory.Exists(latestPackageLocation))
            {
                Directory.CreateDirectory(latestPackageLocation);
            }

            string installMarkerPath = Path.Combine(latestPackageLocation, "installed.txt");
            File.WriteAllText(installMarkerPath, package.Version.ToString());
        }
    }
}