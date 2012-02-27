using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public IList<string> GetWatchedPackages(string environmentName)
        {
            var firstOrDefault = GlobalAgentConfiguration.Environments.FirstOrDefault(x => x.Name == environmentName);
            if (firstOrDefault != null)
            {
                return firstOrDefault.Packages;
            }

            return new string[0];
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
