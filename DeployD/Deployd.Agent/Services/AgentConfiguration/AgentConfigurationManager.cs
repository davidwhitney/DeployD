using System.Collections.Generic;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationManager : IAgentConfigurationManager
    {
        public IList<string> WatchedPackages
        {
            get { return new[]{ "justgiving-sdk"}; }
        }
    }
}
