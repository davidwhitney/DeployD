using System;
using System.IO;
using System.Linq;
using Deployd.Core.Queries;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationDownloader : IAgentConfigurationDownloader
    {
        private const string DEPLOYD_CONFIGURATION_PACKAGE_NAME = "Deployd.Configuration";

        private readonly IRetrieveAllAvailablePackageManifestsQuery _packageQuery;

        public AgentConfigurationDownloader(IRetrieveAllAvailablePackageManifestsQuery packageQuery)
        {
            _packageQuery = packageQuery;
        }

        public void DownloadAgentConfiguration(string targetFile)
        {
            var configPackage = _packageQuery.GetLatestPackage(DEPLOYD_CONFIGURATION_PACKAGE_NAME).FirstOrDefault();

            if (configPackage == null)
            {
                throw new InvalidOperationException("No package configuration was found. Node will not sync.");
            }

            var files = configPackage.GetFiles();
            var agentConfigurationFileStream = files.Where(x => x.Path == targetFile).ToList()[0].GetStream();

            var memoryStream = new MemoryStream();
            agentConfigurationFileStream.CopyTo(memoryStream);
            File.WriteAllBytes("AgentConfiguration.xml", memoryStream.ToArray());
        }
    }
}