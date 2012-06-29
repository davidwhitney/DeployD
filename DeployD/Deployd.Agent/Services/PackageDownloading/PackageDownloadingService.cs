using System;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using Deployd.Core.PackageTransport;
using Deployd.Core.Remoting;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.Services.PackageDownloading
{
    public class PackageDownloadingService : IWindowsService
    {
        private readonly IAgentConfigurationManager _agentConfigurationManager; 
        protected readonly ILogger _logger;
        private readonly IHubCommunicator _hubCommunicator;
        private readonly IInstalledPackageArchive _installCache;
        private readonly RunningInstallationTaskList _runningTasks;

        public ApplicationContext AppContext { get; set; }

        private readonly IAgentSettingsManager _settingsManager;
        protected readonly IRetrievePackageQuery AllPackagesQuery;
        protected readonly ILocalPackageCache AgentCache;

        public TimedSingleExecutionTask TimedTask { get; private set; }

        public PackageDownloadingService(IAgentSettingsManager agentSettingsManager, 
            IRetrievePackageQuery allPackagesQuery, 
            ILocalPackageCache agentCache, 
            IAgentConfigurationManager agentConfigurationManager,
            ILogger logger,
            IHubCommunicator hubCommunicator,
            IInstalledPackageArchive installCache)
        {
            _settingsManager = agentSettingsManager;
            AllPackagesQuery = allPackagesQuery;
            AgentCache = agentCache;
            _agentConfigurationManager = agentConfigurationManager;
            _logger = logger;
            _hubCommunicator = hubCommunicator;
            _installCache = installCache;
            TimedTask = new TimedSingleExecutionTask(agentSettingsManager.Settings.PackageSyncIntervalMs, FetchPackages, _logger);

            AgentCache.OnUpdateStarted += (sender, args) => _hubCommunicator.SendStatusToHub(AgentStatusFactory.BuildStatus(AgentCache, _installCache, _runningTasks, _settingsManager));
            AgentCache.OnUpdateFinished += (sender, args) => _hubCommunicator.SendStatusToHub(AgentStatusFactory.BuildStatus(AgentCache, _installCache, _runningTasks, _settingsManager));
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
