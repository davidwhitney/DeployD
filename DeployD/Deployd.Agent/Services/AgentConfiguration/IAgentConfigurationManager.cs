using System.Collections.Generic;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public interface IAgentConfigurationManager
    {
        IList<string> WatchedPackages { get; } 
    }
}