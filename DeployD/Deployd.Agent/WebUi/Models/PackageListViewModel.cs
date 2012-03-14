using System.Collections.Generic;
using System.Linq;
using Deployd.Core.Installation;
using NuGet;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageListViewModel
    {
        public IList<LocalPackageInformation> Packages { get; set; }

        public IList<InstallTaskViewModel> CurrentTasks { get; set; }

        public IEnumerable<string> AvailableVersions { get; set; }

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