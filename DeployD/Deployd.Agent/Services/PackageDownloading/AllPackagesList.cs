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

        public AllPackagesList(IAgentConfigurationManager agentConfiguration, IAgentSettings agentSettings)
        {
            _agentConfiguration = agentConfiguration;
            _agentSettings = agentSettings;
        }

        public IEnumerable<IPackage> GetWatched()
        {
            var watched = _agentConfiguration.GetWatchedPackages(_agentSettings.DeploymentEnvironment);
            return this.Where(p => watched.Contains(p.Id));
        }
    }
}