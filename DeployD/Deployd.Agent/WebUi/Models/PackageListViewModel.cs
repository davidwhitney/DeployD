using System.Collections.Generic;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageListViewModel
    {
        public IList<LocalPackageInformation> Packages { get; set; }

        public PackageListViewModel()
        {
            Packages = new List<LocalPackageInformation>();
        }
    }

    public class LocalPackageInformation
    {
        public string PackageId { get; set; }
        public string InstalledVersion { get; set; }
        public string LatestAvailableVersion { get; set; }
        public List<string> AvailableVersions { get; set; } 
    }
}