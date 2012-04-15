using System.Collections.Generic;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageListViewModel
    {
        public IList<LocalPackageInformation> Packages { get; set; }
        public IList<InstallTaskViewModel> CurrentTasks { get; set; }
        public IEnumerable<string> AvailableVersions { get; set; }

        public string NugetRepository { get; set; }

        public PackageListViewModel()
        {
            Packages = new List<LocalPackageInformation>();
            CurrentTasks = new List<InstallTaskViewModel>();
            AvailableVersions = new List<string>();
        }
    }
}
