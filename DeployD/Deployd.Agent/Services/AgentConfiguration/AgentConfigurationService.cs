using Deployd.Core;
using Deployd.Core.Hosting;
using log4net;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationService : IWindowsService
    {
        protected static readonly ILog Logger = LogManager.GetLogger("AgentConfigurationService");
        private const string AGENT_CONFIGURATION_FILE = "GlobalAgentConfiguration.xml";
        
        public ApplicationContext AppContext { get; set; }

        private readonly IAgentConfigurationDownloader _configurationDownloader;
        private readonly TimedSingleExecutionTask _task;

        public AgentConfigurationService(IAgentConfigurationDownloader configurationDownloader)
        {
            _configurationDownloader = configurationDownloader;
            _task = new TimedSingleExecutionTask(60000, DownloadConfiguration, true);
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
            _configurationDownloader.DownloadAgentConfiguration(AGENT_CONFIGURATION_FILE);
        }
    }
}