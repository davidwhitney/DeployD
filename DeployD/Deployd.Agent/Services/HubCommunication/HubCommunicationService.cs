using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using Deployd.Core.Remoting;
using Ninject.Extensions.Logging;
using log4net;

namespace Deployd.Agent.Services.HubCommunication
{
    public class HubCommunicationService : IWindowsService
    {
        private readonly IHubCommunicator _hubCommunicator;
        private readonly IPackagesList _allPackagesList;
        private readonly ILocalPackageCache _localPackageCache;
        private Timer _pingTimer = null;
        private const int PingIntervalInMilliseconds = 15000;
        private readonly IInstalledPackageArchive _installCache;
        private readonly RunningInstallationTaskList _runningTasks;
        private readonly IAgentSettingsManager _settingsManager;
        private readonly ILogger _logger;
        private readonly CurrentlyDownloadingList _currentlyDownloadingList;

        public HubCommunicationService(IHubCommunicator hubCommunicator,
            IPackagesList allPackagesList, 
            ILocalPackageCache localPackageCache,
            IInstalledPackageArchive installCache, 
            RunningInstallationTaskList runningTasks, 
            IAgentSettingsManager settingsManager, 
            ILogger logger,
            CurrentlyDownloadingList currentlyDownloadingList)
        {
            _hubCommunicator = hubCommunicator;
            _allPackagesList = allPackagesList;
            _localPackageCache = localPackageCache;
            _installCache = installCache;
            _runningTasks = runningTasks;
            _settingsManager = settingsManager;
            _logger = logger;
            _currentlyDownloadingList = currentlyDownloadingList;
        }

        ~HubCommunicationService()
        {
            _logger.Warn("Destroying a {0}", this.GetType());

        }

        public void Start(string[] args)
        {
            _pingTimer = new Timer(PingIntervalInMilliseconds);
            _pingTimer.Elapsed += SendStatusToHub;
            _pingTimer.Enabled = true;

            // say hello immediately
            SendStatusToHub(this, null);
        }

        public void SendStatusToHub(object sender, ElapsedEventArgs e)
        {
            _hubCommunicator.SendStatusToHub(AgentStatusFactory.BuildStatus(_allPackagesList, _localPackageCache, _installCache, _runningTasks, _settingsManager, _currentlyDownloadingList));
        }

        public void Stop()
        {
            _pingTimer.Enabled = false;
            _pingTimer.Dispose();
            _pingTimer = null;
        }

        public ApplicationContext AppContext { get; set; }

    }
}
