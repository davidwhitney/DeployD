using System.Collections.Generic;
using Deployd.Core;
using NuGet;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageListViewModel
    {
        public IList<LocalPackageInformation> Packages { get; set; }
        public IList<InstallTaskViewModel> CurrentTasks { get; set; }
        public IEnumerable<string> AvailableVersions { get; set; }
        public List<string> Tags { get; set; }

        public string NugetRepository { get; set; }

        public IList<IPackage> Updating { get; set; }

        public PackageListViewModel()
        {
            Packages = new List<LocalPackageInformation>();
            CurrentTasks = new List<InstallTaskViewModel>();
            Tags = new List<string>();
            AvailableVersions = new List<string>();
        }

    }
}
