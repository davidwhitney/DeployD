using System;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Ninject.Extensions.Logging;
using log4net;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationService : IWindowsService
    {
        public ApplicationContext AppContext { get; set; }
        public TimedSingleExecutionTask TimedTask { get; private set; }

        private readonly IAgentConfigurationDownloader _configurationDownloader;
        private readonly ILogger _logger;

        public AgentConfigurationService(IAgentSettings agentSettings, IAgentConfigurationDownloader configurationDownloader, ILogger logger)
        {
            if (agentSettings == null) throw new ArgumentNullException("agentSettings");
            if (configurationDownloader == null) throw new ArgumentNullException("configurationDownloader");
            _configurationDownloader = configurationDownloader;
            _logger = logger;
            TimedTask = new TimedSingleExecutionTask(agentSettings.ConfigurationSyncIntervalMs, DownloadConfiguration,logger, true);
        }

        ~AgentConfigurationService()
        {
            _logger.Warn("Destroying a {0}", this.GetType());

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
            _logger.Debug("Downloading configuration");
            _configurationDownloader.DownloadAgentConfiguration();
        }
    }
}