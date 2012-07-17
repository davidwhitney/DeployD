using System.Collections.Generic;
using System.Linq;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.PackageCaching;
using NuGet;

namespace Deployd.Agent.Services.PackageDownloading
{
    public class AllPackagesList : List<IPackage>, IPackagesList
    {
        private readonly IAgentConfigurationManager _agentConfiguration;
        private readonly IAgentSettings _agentSettings;
        private IList<string> _watchedPackages;

        public AllPackagesList(IAgentConfigurationManager agentConfiguration, IAgentSettings agentSettings)
        {
            _agentConfiguration = agentConfiguration;
            _agentSettings = agentSettings;
            _watchedPackages = _agentConfiguration.GetWatchedPackages(_agentSettings.DeploymentEnvironment);
        }

        public IEnumerable<IPackage> GetWatched()
        {
            return this.Where(p => _watchedPackages.Contains(p.Id));
        }
    }
}