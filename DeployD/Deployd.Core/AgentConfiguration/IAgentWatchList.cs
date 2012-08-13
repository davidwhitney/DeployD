using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentWatchList
    {
        string[] Groups { get; set; }
        string[] Packages { get; set; }
    }
}
