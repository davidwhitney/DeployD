using System;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using log4net;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationService : IWindowsService
    {
        protected static readonly ILog Logger = LogManager.GetLogger("AgentConfigurationService");
        public ApplicationContext AppContext { get; set; }

        private readonly IAgentConfigurationDownloader _configurationDownloader;
        private readonly TimedSingleExecutionTask _task;

        public AgentConfigurationService(IAgentSettings agentSettings, IAgentConfigurationDownloader configurationDownloader)
        {
            if (agentSettings == null) throw new ArgumentNullException("agentSettings");
            if (configurationDownloader == null) throw new ArgumentNullException("configurationDownloader");

            _configurationDownloader = configurationDownloader;
            _task = new TimedSingleExecutionTask(agentSettings.ConfigurationSyncIntervalMs, DownloadConfiguration, true);
        }

        public void Start(string[] args)
        {
            _task.Start(args);
        }

        public void Stop()
        {
            _task.Stop();
        }

        public void DownloadConfiguration()
        {
            Logger.DebugFormat("Downloading " + ConfigurationFiles.AGENT_CONFIGURATION_FILE);
            _configurationDownloader.DownloadAgentConfiguration();
        }
    }
}