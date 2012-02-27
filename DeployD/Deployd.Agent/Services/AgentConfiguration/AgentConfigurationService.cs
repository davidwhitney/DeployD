using System.Collections.Generic;
using Deployd.Core.Caching;
using Deployd.Core.Queries;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationService : PackageSyncServiceBase
    {
        public AgentConfigurationService(IRetrieveAllAvailablePackageManifestsQuery allPackagesQuery, INuGetPackageCache agentCache)
            : base(allPackagesQuery, agentCache, 60000)
        {
        } 

        public override IList<string> GetPackagesToDownload()
        {
            return new[]{"Deployd.Configuration"};
        }
    }
}
