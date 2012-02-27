using System.Collections.Generic;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.Caching;
using Deployd.Core.Queries;

namespace Deployd.Agent.Services.PackageDownloading
{
    public class PackageDownloadingService : PackageSyncServiceBase
    {
        private readonly IAgentConfigurationManager _agentConfigurationManager;

        public PackageDownloadingService(IRetrieveAllAvailablePackageManifestsQuery allPackagesQuery, INuGetPackageCache agentCache, IAgentConfigurationManager agentConfigurationManager)
            : base(allPackagesQuery, agentCache)
        {
            _agentConfigurationManager = agentConfigurationManager;
        } 

        public override IList<string> GetPackagesToDownload()
        {
            return _agentConfigurationManager.WatchedPackages;
        }
    }
}
