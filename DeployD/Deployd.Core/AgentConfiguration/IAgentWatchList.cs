using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentWatchList
    {
        List<string> Groups { get; set; }
        List<WatchPackage> Packages { get; set; }
    }
}
