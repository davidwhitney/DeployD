using System.Collections.Generic;
using System.Runtime.Serialization;
using Deployd.Core.Installation;

namespace Deployd.Core
{
    [DataContract(Name="package")]
    public class LocalPackageInformation
    {
        [DataMember(Name="packageId")]
        public string PackageId { get; set; }
        [DataMember(Name = "installedVersion")]
        public string InstalledVersion { get; set; }
        [DataMember(Name = "isInstalled")]
        public bool Installed { get { return !string.IsNullOrWhiteSpace(InstalledVersion); } set{} }
        [DataMember(Name = "latestAvailableVersion")]
        public string LatestAvailableVersion { get; set; }
        [DataMember(Name = "availableVersions")]
        public List<string> AvailableVersions { get; set; }
        [DataMember(Name = "currentTask")]
        public InstallTaskViewModel CurrentTask { get; set; }
        [DataMember(Name = "lastInstallationTask")]
        public InstallationTask LastInstallationTask { get; set; }
        [DataMember(Name="tags")]
        public string[] Tags { get; set; }
    }
}