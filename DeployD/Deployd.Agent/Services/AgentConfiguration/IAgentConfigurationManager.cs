using System.Collections.Generic;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public interface IAgentConfigurationManager
    {
        IList<string> GetWatchedPackages(string environmentName);
        GlobalAgentConfiguration ReadFromDisk(string fileName = null);
        void SaveToDisk(GlobalAgentConfiguration configuration, string fileName = null);
        void SaveToDisk(byte[] configuration, string fileName = null);
        string ApplicationFilePath(string agentConfigurationFile);
    }
}