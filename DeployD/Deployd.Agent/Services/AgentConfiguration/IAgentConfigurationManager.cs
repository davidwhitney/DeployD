using System.Collections.Generic;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public interface IAgentConfigurationManager
    {
        IList<string> GetWatchedPackages(string environmentName);
        GlobalAgentConfiguration ReadFromDisk(string fileName = ConfigurationFiles.AgentConfigurationFile);
        void SaveToDisk(GlobalAgentConfiguration configuration, string fileName = ConfigurationFiles.AgentConfigurationFile);
        void SaveToDisk(byte[] configuration, string fileName = ConfigurationFiles.AgentConfigurationFile);
    }
}