using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationManager : IAgentConfigurationManager
    {
        public IList<string> WatchedPackages
        {
            get { return new[]{ "justgiving-sdk", "GG.Search.Indexing.Service"}; }
        }
    }
}
