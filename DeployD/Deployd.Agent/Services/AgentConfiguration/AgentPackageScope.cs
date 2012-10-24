using System.Collections.Generic;
using System.Linq;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core.AgentConfiguration;
using NuGet;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentPackageScope
    {
        private readonly IAgentWatchList _agentWatchList;
        private readonly IPackageGroupConfiguration _packageGroupConfiguration;
        private readonly AllPackagesList _allPackages;

        public AgentPackageScope(IAgentWatchList agentWatchList, IPackageGroupConfiguration packageGroupConfiguration, AllPackagesList allPackages)
        {
            _agentWatchList = agentWatchList;
            _packageGroupConfiguration = packageGroupConfiguration;
            _allPackages = allPackages;
        }

        public string[] GetPackagesWithinScope()
        {
            var packageIds = new List<WatchPackage>();
            if (_packageGroupConfiguration.Groups != null && _packageGroupConfiguration.Groups.Length > 0)
            {
                IEnumerable<PackageGroup> groups = _packageGroupConfiguration.Groups.Where(g => _agentWatchList.Groups.Contains(g.GroupName));
                foreach(var group in groups)
                {
                    packageIds.AddRange(group.PackageIds.Select(packageId=>new WatchPackage(){Name=packageId}));
                }
            }

            if (_agentWatchList.Packages != null && _agentWatchList.Packages.Count > 0)
            {
                packageIds.AddRange(_agentWatchList.Packages);
            }

            return packageIds.Select(p=>p.Name).ToArray();
        }
    }
}