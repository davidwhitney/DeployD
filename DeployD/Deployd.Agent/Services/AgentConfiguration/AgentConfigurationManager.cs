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
            if (firstOrDefault != null)
            {
                return firstOrDefault.Packages;
            }

            return new string[0];
        }

        public GlobalAgentConfiguration ReadFromDisk(string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE)
        {
            lock (_fileLock)
            {
                GlobalAgentConfiguration config;

                using (var fs = new FileStream(fileName, FileMode.Open))
                {
                    config =
                        (GlobalAgentConfiguration) new XmlSerializer(typeof (GlobalAgentConfiguration)).Deserialize(fs);
                    fs.Close();
                }

                return config;
            }
        }

        public void SaveToDisk(GlobalAgentConfiguration configuration, string fileName = ConfigurationFiles.AGENT_CONFIGURATION_FILE)
        {
            lock (_fileLock)
            {
                using (var writer = new StreamWriter(fileName))
                {
                    new XmlSerializer(typeof (GlobalAgentConfiguration)).Serialize(writer, configuration);
                    writer.Close();
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
