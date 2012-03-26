using System.Collections.Generic;

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
            CurrentTasks = new List<InstallTaskViewModel>();
            AvailableVersions = new List<string>();
        }
    }

    public class LocalPackageInformation
    {
        public string PackageId { get; set; }
        public string InstalledVersion { get; set; }
        public string LatestAvailableVersion { get; set; }
        public List<string> AvailableVersions { get; set; }
        public InstallTaskViewModel CurrentTask { get; set; }

        public LocalPackageInformation()
        {
            AvailableVersions = new List<string>();
        }
    }
}