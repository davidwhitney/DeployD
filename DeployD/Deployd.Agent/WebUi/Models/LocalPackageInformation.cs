using System.Collections.Generic;

namespace Deployd.Agent.WebUi.Models
{
    public class LocalPackageInformation
    {
        public string PackageId { get; set; }
        public string InstalledVersion { get; set; }
        public bool Installed { get { return !string.IsNullOrWhiteSpace(InstalledVersion); } }
        public string LatestAvailableVersion { get; set; }
        public List<string> AvailableVersions { get; set; }
        public InstallTaskViewModel CurrentTask { get; set; }
    }
}