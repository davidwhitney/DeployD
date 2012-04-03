using System.Linq;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Deployd.Core.Queries;
using log4net;

namespace Deployd.Agent.Services.PackageDownloading
{
    public class PackageDownloadingService : IWindowsService
    {
        private readonly IAgentConfigurationManager _agentConfigurationManager; 
        protected static readonly ILog Logger = LogManager.GetLogger("PackageDownloadingService");

        public ApplicationContext AppContext { get; set; }

        private readonly IAgentSettings _settings;
        protected readonly IRetrievePackageQuery AllPackagesQuery;
        protected readonly INuGetPackageCache AgentCache;

        public TimedSingleExecutionTask TimedTask { get; private set; }

        public PackageDownloadingService(IAgentSettings agentSettings, IRetrievePackageQuery allPackagesQuery, INuGetPackageCache agentCache, IAgentConfigurationManager agentConfigurationManager)
        {
            _settings = agentSettings;
            AllPackagesQuery = allPackagesQuery;
            AgentCache = agentCache;
            _agentConfigurationManager = agentConfigurationManager;
            TimedTask = new TimedSingleExecutionTask(agentSettings.PackageSyncIntervalMs, FetchPackages);
        }

        public void Start(string[] args)
        {
            TimedTask.Start(args);
        }

        public void Stop()
        {
            TimedTask.Stop();
        }

        public void FetchPackages()
        {
            var packages = _agentConfigurationManager.GetWatchedPackages(_settings.DeploymentEnvironment);
            
            foreach (var latestPackageOfType in packages.Select(packageId => AllPackagesQuery.GetLatestPackage(packageId)).Where(latestPackageOfType => latestPackageOfType != null))
            {
                AgentCache.Add(latestPackageOfType);
            }
        }
    }
}
