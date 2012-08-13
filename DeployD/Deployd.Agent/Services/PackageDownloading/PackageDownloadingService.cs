using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Agent.Services.HubCommunication;
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
        private readonly IPackageRepository _packageRepository;
        private readonly IPackagesList _allPackagesList;
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
                                         IInstalledPackageArchive installCache,
                                         IPackageRepositoryFactory packageRepositoryFactory,
                                        IPackagesList allPackagesList)
        {
            _settingsManager = agentSettingsManager;
            AllPackagesQuery = allPackagesQuery;
            AgentCache = agentCache;
            _agentConfigurationManager = agentConfigurationManager;
            _logger = logger;
            _hubCommunicator = hubCommunicator;
            _installCache = installCache;
            _packageRepository = packageRepositoryFactory.CreateRepository(agentSettingsManager.Settings.NuGetRepository);
            _allPackagesList = allPackagesList;
            TimedTask = new TimedSingleExecutionTask(agentSettingsManager.Settings.PackageSyncIntervalMs, FetchPackages,
                                                     _logger);

            AgentCache.OnUpdateStarted +=
                (sender, args) =>
                _hubCommunicator.SendStatusToHub(AgentStatusFactory.BuildStatus(_allPackagesList, AgentCache, _installCache, _runningTasks,
                                                                                _settingsManager));
            AgentCache.OnUpdateFinished +=
                (sender, args) =>
                _hubCommunicator.SendStatusToHub(AgentStatusFactory.BuildStatus(_allPackagesList, AgentCache, _installCache, _runningTasks,
                                                                                _settingsManager));
        }

        ~PackageDownloadingService()
        {
            _logger.Warn("Destroying a {0}", this.GetType());
        }

        public void Start(string[] args)
        {
            TimedTask.Start(args);

            var packages = _agentConfigurationManager.GetWatchedPackages(_settingsManager.Settings.DeploymentEnvironment);
            _logger.Debug("Downloading service will download the following packages:");
            foreach(var package in packages)
            {
                _logger.Debug(package);
            }

            // do an initial check/fetch
            FetchPackages();
        }

        public void Stop()
        {
            TimedTask.Stop();
        }

        public void FetchPackages()
        {
            var packages = _agentConfigurationManager.GetWatchedPackages(_settingsManager.Settings.DeploymentEnvironment);

            // todo: this should probably only clean/update the packages that have changed
            _allPackagesList.Clear();
            _allPackagesList.AddRange(_packageRepository.GetPackages());
            _logger.Debug("added {0} packages to all packages list", _allPackagesList.Count);

            foreach (var packageId in packages)
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