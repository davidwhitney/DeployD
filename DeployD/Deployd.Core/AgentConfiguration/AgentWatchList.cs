using System.Xml.Serialization;

namespace Deployd.Core.AgentConfiguration
{
    [XmlRoot("watch")]
    public class AgentWatchList : IAgentWatchList
    {
        [XmlElement("groups")]
        public string[] Groups { get; set; }

        [XmlElement("packages")]
        public string[] Packages { get; set; }
    }
}