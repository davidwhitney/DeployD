using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Ninject.Extensions.Logging;
using log4net;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationManager : IAgentConfigurationManager
    {
        private readonly object _fileLock;
        private readonly ILogger _logger;

        public AgentConfigurationManager(ILogger logger)
        {
            _logger = logger;
            _fileLock = new object();
        }

        public GlobalAgentConfiguration GlobalAgentConfiguration
        {
            get { return ReadFromDisk(); }
        }

        private string ApplicationFilePath(string fileName)
        {
            return Path.Combine(ConfigurationFiles.AgentConfigurationFileLocation.MapVirtualPath(), fileName);
            
        }

        public IList<string> GetWatchedPackages(string environmentName)
        {
            var firstOrDefault = GlobalAgentConfiguration.Environments.FirstOrDefault(x => x.Name == environmentName);
            return firstOrDefault != null ? firstOrDefault.Packages : new List<string>();
        }

        public GlobalAgentConfiguration ReadFromDisk(string fileName = ConfigurationFiles.AgentConfigurationFile)
        {
            var configurationPath = ApplicationFilePath(fileName);
            lock (_fileLock)
            {
                using (var fs = new FileStream(configurationPath, FileMode.Open))
                {
                    return (GlobalAgentConfiguration) new XmlSerializer(typeof (GlobalAgentConfiguration)).Deserialize(fs);
                }
            }
        }

        public void SaveToDisk(GlobalAgentConfiguration configuration, string fileName = ConfigurationFiles.AgentConfigurationFile)
        {
            var configurationPath = ApplicationFilePath(fileName);
            _logger.Debug(string.Format("saving configuration file to {0}", configurationPath));
            
            lock (_fileLock)
            {
                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream))
                {
                    new XmlSerializer(typeof (GlobalAgentConfiguration)).Serialize(writer, configuration);
                    SaveToDisk(memoryStream.ToArray(), configurationPath);
                }
            }
        }

        public void SaveToDisk(byte[] configuration, string fileName = ConfigurationFiles.AgentConfigurationFile)
        {
            var configurationPath = ApplicationFilePath(fileName);
            _logger.Debug(string.Format("saving configuration file to {0}", configurationPath));
           
            try
            {
                lock (_fileLock)
                {
                    File.WriteAllBytes(configurationPath, configuration);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not save configuration file " + fileName.MapVirtualPath());
            }
        }
    }
}
