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
        public TimedSingleExecutionTask TimedTask { get; private set; }

        private readonly IAgentConfigurationDownloader _configurationDownloader;

        public AgentConfigurationService(IAgentSettings agentSettings, IAgentConfigurationDownloader configurationDownloader)
        {
            if (agentSettings == null) throw new ArgumentNullException("agentSettings");
            if (configurationDownloader == null) throw new ArgumentNullException("configurationDownloader");

            _configurationDownloader = configurationDownloader;
            TimedTask = new TimedSingleExecutionTask(agentSettings.ConfigurationSyncIntervalMs, DownloadConfiguration, true);
        }

        public void Start(string[] args)
        {
            TimedTask.Start(args);
        }

        public void Stop()
        {
            TimedTask.Stop();
        }

        public void DownloadConfiguration()
        {
            Logger.DebugFormat("Downloading " + ConfigurationFiles.AGENT_CONFIGURATION_FILE);
            _configurationDownloader.DownloadAgentConfiguration();
        }
    }
}