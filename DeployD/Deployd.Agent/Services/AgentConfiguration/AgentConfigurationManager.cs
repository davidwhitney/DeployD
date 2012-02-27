using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationManager : IAgentConfigurationManager
    {
        public GlobalAgentConfiguration GlobalAgentConfiguration
        {
            get { return ReadFromDisk(ConfigurationFiles.AGENT_CONFIGURATION_FILE); }
        }

        public IList<string> GetWatchedPackages()
        {
            return new[] {"justgiving-sdk"};
        }

        public GlobalAgentConfiguration ReadFromDisk(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open);
            return (GlobalAgentConfiguration)new XmlSerializer(typeof(GlobalAgentConfiguration)).Deserialize(fs);
        }

        public void SaveToDisk(GlobalAgentConfiguration configuration, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                new XmlSerializer(typeof(GlobalAgentConfiguration)).Serialize(writer, configuration);
                writer.Close();
            }
        }
    }
}
