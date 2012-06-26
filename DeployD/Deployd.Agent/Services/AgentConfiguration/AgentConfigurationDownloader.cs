using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.PackageTransport;
using NuGet;
using log4net;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationDownloader : IAgentConfigurationDownloader
    {
        public const string DeploydConfigurationPackageName = "Deployd.Configuration";
        private readonly IAgentConfigurationManager _agentConfigurationManager;
        private readonly IRetrievePackageQuery _packageQuery;
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly ILogger _logger;

        public AgentConfigurationDownloader(IAgentConfigurationManager agentConfigurationManager,
                                            IRetrievePackageQuery packageQuery,
                                            IAgentSettingsManager agentSettingsManager,
            ILogger logger)
        {
            _agentConfigurationManager = agentConfigurationManager;
            _packageQuery = packageQuery;
            _agentSettingsManager = agentSettingsManager;
            _logger = logger;
        }

        public void DownloadAgentConfiguration()
        {
            IPackage configPackage = null;
            try
            {
                configPackage = DownloadConfigurationPackage();
            } catch(Exception ex)
            {
                _logger.Error("Could not download configuration package", ex);
                return;
            }

            try
            {
                var agentConfigurationFile = ExtractConfig(configPackage);
                SaveToDisk(agentConfigurationFile);
            }
            catch (Exception ex)
            {
                _logger.Error("failed", ex);
            }
        }

        private IPackage DownloadConfigurationPackage()
        {
            using (new DebugTimer("Downloading " + DeploydConfigurationPackageName))
            {
                var configPackage = _packageQuery.GetLatestPackage(DeploydConfigurationPackageName);

                if (configPackage == null)
                {
                    throw new AgentConfigurationPackageNotFoundException(DeploydConfigurationPackageName);
                }

                return configPackage;
            }
        }

        private static IPackageFile ExtractConfig(IPackage configPackage)
        {
            using (new DebugTimer("Extracting config from " + DeploydConfigurationPackageName))
            {
                var files = configPackage.GetFiles();
                var agentConfigurationFile = ExtractAgentConfigurationFile(ConfigurationFiles.AgentConfigurationFile, files);
                return agentConfigurationFile;
            }
        }

        private void SaveToDisk(IPackageFile agentConfigurationFile)
        {
            using (new DebugTimer("Saving config from " + DeploydConfigurationPackageName))
            {
                var configBytes = GetConfigurationFileAsBytes(agentConfigurationFile);
                _agentConfigurationManager.SaveToDisk(configBytes);
                _agentSettingsManager.UnloadSettings();
            }
        }

        private static byte[] GetConfigurationFileAsBytes(IPackageFile agentConfigurationFile)
        {
            var agentConfigurationFileStream = agentConfigurationFile.GetStream();

            byte[] configBytes;

            using (var memoryStream = new MemoryStream())
            {
                agentConfigurationFileStream.CopyTo(memoryStream);
                configBytes = memoryStream.ToArray();
                memoryStream.Close();
            }
            return configBytes;
        }

        private static IPackageFile ExtractAgentConfigurationFile(string targetFile, IEnumerable<IPackageFile> files)
        {
            var agentConfigFileList = files.Where(x => x.Path == targetFile).ToList();

            if (agentConfigFileList.Count == 0)
            {
                throw new AgentConfigurationNotFoundException(DeploydConfigurationPackageName, targetFile);
            }

            return agentConfigFileList[0];
        }
    }
}