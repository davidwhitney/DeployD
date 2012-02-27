using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Deployd.Core.Caching;
using Deployd.Core.Queries;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationService : PackageSyncServiceBase
    {
        private const string AGENT_CONFIGURATION_FILE = "AgentConfiguration.xml";
        private const string DEPLOYD_CONFIGURATION_PACKAGE_NAME = "Deployd.Configuration";

        public AgentConfigurationService(IRetrieveAllAvailablePackageManifestsQuery allPackagesQuery, INuGetPackageCache agentCache)
            : base(allPackagesQuery, agentCache, 60000)
        {
        }

        public override void FetchPackages(object sender, ElapsedEventArgs e)
        {
            FetchPackages();
        }

        public override void FetchPackages()
        {
            OneAtATime(()=>
            {
                var configPackage = _allPackagesQuery.GetLatestPackage(DEPLOYD_CONFIGURATION_PACKAGE_NAME).FirstOrDefault();

                if (configPackage == null)
                {
                    throw new InvalidOperationException("No package configuration was found. Node will not sync.");
                }

                var files = configPackage.GetFiles();
                var agentConfigurationFileStream = files.Where(x => x.Path == AGENT_CONFIGURATION_FILE).ToList()[0].GetStream();

                var memoryStream = new MemoryStream();
                agentConfigurationFileStream.CopyTo(memoryStream);
                File.WriteAllBytes("AgentConfiguration.xml", memoryStream.ToArray());

            });
        }

        public override IList<string> GetPackagesToDownload()
        {
            return new List<string>{DEPLOYD_CONFIGURATION_PACKAGE_NAME};
        }
    }
}
