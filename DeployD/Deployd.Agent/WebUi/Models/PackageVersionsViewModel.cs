using System.Collections.Generic;
using Deployd.Core.Installation;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageVersionsViewModel
    {
        public string PackageName { get; set; }
        public string CurrentInstalledVersion { get; set; }
        public InstallationTask CurrentInstallTask { get; set; }
        public List<string> CachedVersions { get; set; }

        public PackageVersionsViewModel(string packageName, IEnumerable<string> inner, string currentInstalledVersion, InstallationTask currentInstallTask)
        {
            PackageName = packageName;
            CurrentInstalledVersion = currentInstalledVersion;
            CurrentInstallTask = currentInstallTask;
            CachedVersions = new List<string>(inner);
        }
    }
}