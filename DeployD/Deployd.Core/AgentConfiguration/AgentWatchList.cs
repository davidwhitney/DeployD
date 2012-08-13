using System.Xml.Serialization;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentWatchList : IAgentWatchList
    {
        public string[] Groups { get; set; }

        public string[] Packages { get; set; }
    }
}