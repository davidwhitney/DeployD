using System.Collections.Generic;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public interface IAgentConfigurationManager
    {
        IList<string> GetWatchedPackages(string environmentName);
        GlobalAgentConfiguration ReadFromDisk(string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE);
        void SaveToDisk(GlobalAgentConfiguration configuration, string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE);
        void SaveToDisk(byte[] configuration, string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE);
    }
}