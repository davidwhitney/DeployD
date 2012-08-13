using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentWatchListManager
    {
        IAgentWatchList Build();
        void SaveWatchList(IAgentWatchList agentWatchList);
        void SaveWatchList(string agentWatchList);
    }

    public class AgentWatchListManager : IAgentWatchListManager
    {
        private static object _fileLock=new object();
        public IAgentWatchList Build()
        {
            lock (_fileLock)
            {
                using (var fs = new FileStream("~\\watchList.config".MapVirtualPath(), FileMode.Open))
                {
                    return (AgentWatchList)new XmlSerializer(typeof(AgentWatchList)).Deserialize(fs);
                }
            }
        }

        public void SaveWatchList(IAgentWatchList agentWatchList)
        {
            lock (_fileLock)
            {
                using (var fs = new FileStream("~\\watchList.config".MapVirtualPath(), FileMode.Create))
                {
                    new XmlSerializer(typeof(AgentWatchList)).Serialize(fs, agentWatchList);
                    fs.Flush();
                }
            }
        }

        public void SaveWatchList(string agentWatchList)
        {
            
            using (XmlReader reader = XmlReader.Create(new StringReader(agentWatchList)))
            {
                var serialized = new XmlSerializer(typeof (AgentWatchList)).Deserialize(reader);
            }
            lock (_fileLock)
            {

                using (var fs = new FileStream("~\\watchList.config".MapVirtualPath(), FileMode.Create))
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(agentWatchList);
                    fs.Flush();
                }
            }
        }
    }
}