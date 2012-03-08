using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deployd.Core.Queries;
using NuGet;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationDownloader : IAgentConfigurationDownloader
    {
        public const string DEPLOYD_CONFIGURATION_PACKAGE_NAME = "Deployd.Configuration";

        private readonly IAgentConfigurationManager _agentConfigurationManager;
        private readonly IRetrievePackageQuery _packageQuery;

        public AgentConfigurationDownloader(IAgentConfigurationManager agentConfigurationManager,
                                            IRetrievePackageQuery packageQuery)
        {
            _agentConfigurationManager = agentConfigurationManager;
            _packageQuery = packageQuery;
        }

        public void DownloadAgentConfiguration()
        {
            var configPackage = _packageQuery.GetLatestPackage(DEPLOYD_CONFIGURATION_PACKAGE_NAME);

            if (configPackage == null)
            {
                throw new AgentConfigurationPackageNotFoundException(DEPLOYD_CONFIGURATION_PACKAGE_NAME);
            }

            var files = configPackage.GetFiles();
            var agentConfigurationFile = ExtractAgentConfigurationFile(ConfigurationFiles.AGENT_CONFIGURATION_FILE, files);
            var agentConfigurationFileStream = agentConfigurationFile.GetStream();

            byte[] configBytes;
            using (var memoryStream = new MemoryStream())
            {
                agentConfigurationFileStream.CopyTo(memoryStream);
                configBytes = memoryStream.ToArray();
                memoryStream.Close();
            }

            _agentConfigurationManager.SaveToDisk(configBytes);
        }

        private static IPackageFile ExtractAgentConfigurationFile(string targetFile, IEnumerable<IPackageFile> files)
        {
            var agentConfigFileList = files.Where(x => x.Path == targetFile).ToList();

            if (agentConfigFileList.Count == 0)
            {
                throw new AgentConfigurationNotFoundException(DEPLOYD_CONFIGURATION_PACKAGE_NAME, targetFile);
            }

            return agentConfigFileList[0];
        }
    }
}