using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationManager : IAgentConfigurationManager
    {
        private static string _baseUrl = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DeployD.Agent");
        private readonly object _fileLock;
        private ILog _logger = LogManager.GetLogger(typeof (AgentConfigurationManager));

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
            _logger.DebugFormat("saving configuration file to {0}", fileName.MapVirtualPath());
            string configurationPath = Path.Combine(ConfigurationFiles.AGENT_CONFIGURATION_FILE_LOCATION.MapVirtualPath(), fileName);
            try
            {
                lock (_fileLock)
                {
                    File.WriteAllBytes(configurationPath, configuration);
                }
            } catch (Exception ex)
            {
                _logger.Error("Could not save configuration file " + fileName.MapVirtualPath(), ex);
            }
        }
    }
}
