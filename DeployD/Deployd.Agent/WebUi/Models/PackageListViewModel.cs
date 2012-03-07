using System.Collections.Generic;
using NuGet;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageListViewModel
    {
        public List<PackageViewModel> Packages { get; set; }

        public PackageListViewModel()
        {
            Packages = new List<PackageViewModel>();
        }
    }

    public class PackageViewModel
    {
        public string PackageId { get; set; }
        public string InstalledVersion { get; set; }
        public string LatestAvailableVersion { get; set; }
        public List<string> AvailableVersions { get; set; } 
    }
}