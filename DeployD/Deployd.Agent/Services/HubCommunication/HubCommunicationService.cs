using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        private Timer _pingTimer = null;
        private const int PingIntervalInMilliseconds = 15000;
        private readonly ILocalPackageCache _agentCache;
        private readonly IInstalledPackageArchive _installCache;
        private readonly RunningInstallationTaskList _runningTasks;
        private readonly IAgentSettingsManager _settingsManager;
        private readonly ILogger _logger;

        public HubCommunicationService(IHubCommunicator hubCommunicator, ILocalPackageCache agentCache, IInstalledPackageArchive installCache, RunningInstallationTaskList runningTasks, IAgentSettingsManager settingsManager, ILogger logger)
        {
            _hubCommunicator = hubCommunicator;
            _agentCache = agentCache;
            _installCache = installCache;
            _runningTasks = runningTasks;
            _settingsManager = settingsManager;
            _logger = logger;
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
            _hubCommunicator.SendStatusToHub(AgentStatusFactory.BuildStatus(_agentCache, _installCache, _runningTasks, _settingsManager));
        }

        public void SendStatusToHub(object sender, ElapsedEventArgs e)
        {
            _hubCommunicator.SendStatusToHub(AgentStatusFactory.BuildStatus(_agentCache, _installCache, _runningTasks, _settingsManager));
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
