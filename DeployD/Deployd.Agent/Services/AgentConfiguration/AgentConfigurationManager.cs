using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationManager : IAgentConfigurationManager
    {
        private readonly object _fileLock;

        public AgentConfigurationManager()
        {
            _fileLock = new object();
        }

        public GlobalAgentConfiguration GlobalAgentConfiguration
        {
            get { return ReadFromDisk(); }
        }

        public IList<string> GetWatchedPackages(string environmentName)
        {
            var firstOrDefault = GlobalAgentConfiguration.Environments.FirstOrDefault(x => x.Name == environmentName);
            return firstOrDefault != null ? firstOrDefault.Packages : new List<string>();
        }

        public GlobalAgentConfiguration ReadFromDisk(string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE)
        {
            lock (_fileLock)
            {
                using (var fs = new FileStream(fileName, FileMode.Open))
                {
                    return (GlobalAgentConfiguration) new XmlSerializer(typeof (GlobalAgentConfiguration)).Deserialize(fs);
                }
            }
        }

        public void SaveToDisk(GlobalAgentConfiguration configuration, string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE)
        {
            lock (_fileLock)
            {
                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream))
                {
                    new XmlSerializer(typeof (GlobalAgentConfiguration)).Serialize(writer, configuration);
                    SaveToDisk(memoryStream.ToArray(), fileName);
                }
            }
        }

        public void SaveToDisk(byte[] configuration, string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE)
        {
            lock (_fileLock)
            {
                File.WriteAllBytes(fileName, configuration);
            }
        }
    }
}
