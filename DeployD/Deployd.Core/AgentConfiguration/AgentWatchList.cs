using System.Collections.Generic;
using System.Xml.Serialization;

namespace Deployd.Core.AgentConfiguration
{
    [XmlRoot("watch")]
    public class AgentWatchList : IAgentWatchList
    {
        [XmlArray("groups")]
        [XmlArrayItem("group")]
        public List<string> Groups { get; set; }

        [XmlArray("packages")]
        [XmlArrayItem("package")]
        public List<string> Packages { get; set; }
    }

    public class GroupList : List<string>
    {
        public GroupList() : base()
        {
            
        }

        public GroupList(IEnumerable<string> items) : base(items)
        {
        }
    }
}