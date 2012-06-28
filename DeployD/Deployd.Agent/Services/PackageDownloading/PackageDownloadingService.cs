using System;
using System.Linq;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.PackageCaching;
using Deployd.Core.PackageTransport;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.Services.PackageDownloading
{
    public class PackageDownloadingService : IWindowsService
    {
        private readonly IAgentConfigurationManager _agentConfigurationManager; 
        protected readonly ILogger _logger;

        public ApplicationContext AppContext { get; set; }

        private readonly IAgentSettingsManager _settingsManager;
        protected readonly IRetrievePackageQuery AllPackagesQuery;
        protected readonly ILocalPackageCache AgentCache;

        public TimedSingleExecutionTask TimedTask { get; private set; }

        public PackageDownloadingService(IAgentSettingsManager agentSettingsManager, 
            IRetrievePackageQuery allPackagesQuery, 
            ILocalPackageCache agentCache, 
            IAgentConfigurationManager agentConfigurationManager,
            ILogger logger)
        {
            _settingsManager = agentSettingsManager;
            AllPackagesQuery = allPackagesQuery;
            AgentCache = agentCache;
            _agentConfigurationManager = agentConfigurationManager;
            _logger = logger;
            TimedTask = new TimedSingleExecutionTask(agentSettingsManager.Settings.PackageSyncIntervalMs, FetchPackages, _logger);
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
            var packages = _agentConfigurationManager.GetWatchedPackages(_settingsManager.Settings.DeploymentEnvironment);

            foreach(var packageId in packages)
            {
                IPackage latestPackage = null;
                try
                {
                    latestPackage = AllPackagesQuery.GetLatestPackage(packageId);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to download latest version of " + packageId);
                    continue;
                }

                if (latestPackage == null)
                    continue;

                AgentCache.Add(latestPackage);
            }
        }
    }
}
