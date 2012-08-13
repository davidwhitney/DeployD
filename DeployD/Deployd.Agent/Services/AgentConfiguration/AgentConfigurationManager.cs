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
        private readonly IAgentWatchListManager _agentWatchListManager;
        private readonly IConfigurationDefaults _configurationDefaults;
        private GlobalAgentConfiguration _cachedConfiguration = null;

        public AgentConfigurationManager(ILogger logger, IAgentWatchListManager agentWatchListManager, IConfigurationDefaults configurationDefaults)
        {
            _logger = logger;
            _agentWatchListManager = agentWatchListManager;
            _configurationDefaults = configurationDefaults;
            _fileLock = new object();
        }

        public GlobalAgentConfiguration GlobalAgentConfiguration
        {
            get
            {
                if (_cachedConfiguration == null)
                    _cachedConfiguration = ReadFromDisk(ApplicationFilePath(_configurationDefaults.AgentConfigurationFile));
                
                return _cachedConfiguration;
            }
        }

        public string ApplicationFilePath(string fileName)
        {
            return Path.Combine(_configurationDefaults.AgentConfigurationFileLocation.MapVirtualPath(), fileName);
            
        }

        public IList<string> GetWatchedPackages(string environmentName)
        {
            List<string> packages = new List<string>();
            var watchList = _agentWatchListManager.Build();
            if (watchList.Groups != null)
            {
                var groups = GlobalAgentConfiguration.Environments.Where(g => watchList.Groups.Contains(g.Name));
                packages.AddRange(groups.SelectMany(g => g.Packages));
            }

            if (watchList.Packages != null)
                packages.AddRange(watchList.Packages);
            
            return packages;
        }

        public GlobalAgentConfiguration ReadFromDisk(string fileName = null)
        {
            var configurationPath = fileName ?? _configurationDefaults.AgentConfigurationFile;
            lock (_fileLock)
            {
                using (var fs = new FileStream(configurationPath, FileMode.Open))
                {
                    return (GlobalAgentConfiguration) new XmlSerializer(typeof (GlobalAgentConfiguration)).Deserialize(fs);
                }
            }
        }

        public void SaveToDisk(GlobalAgentConfiguration configuration, string fileName = null)
        {
            var configurationPath = fileName ?? _configurationDefaults.AgentConfigurationFile;
            _logger.Debug(string.Format("saving configuration file to {0}", configurationPath));
            
            lock (_fileLock)
            {
                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream))
                {
                    new XmlSerializer(typeof (GlobalAgentConfiguration)).Serialize(writer, configuration);
                    SaveToDisk(memoryStream.ToArray(), ApplicationFilePath(configurationPath));
                }
            }
        }

        public void SaveToDisk(byte[] configuration, string fileName = null)
        {
            var configurationPath = fileName ?? _configurationDefaults.AgentConfigurationFile;
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
