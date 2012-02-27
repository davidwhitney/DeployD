using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Deployd.Core.Caching;
using Deployd.Core.Queries;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class AgentConfigurationService : PackageSyncServiceBase
    {
        public AgentConfigurationService(IRetrieveAllAvailablePackageManifestsQuery allPackagesQuery, INuGetPackageCache agentCache)
            : base(allPackagesQuery, agentCache, 60000)
        {
        } 

        public override IList<string> GetPackagesToDownload()
        {
            return new[] {"Deployd.Configuration"};
        }

        public override void FetchPackages(object sender, ElapsedEventArgs e)
        {
            FetchPackages();
        }

        public override void FetchPackages()
        {
            if (!Monitor.TryEnter(_oneSyncAtATimeLock))
            {
                Logger.Info("Skipping a local cache operation because a previous cache operation is still running.");
                return;
            }

            try
            {
                var configPackage = _allPackagesQuery.GetLatestPackage(GetPackagesToDownload().First()).FirstOrDefault();

                if (configPackage == null)
                {
                    throw new InvalidOperationException("No configuration! Oh my!");
                }
                
                var files = configPackage.GetFiles();

            }
            catch(Exception ex)
            {
                Logger.Error("No package configuration was found. Node will not sync.");
            }
            finally
            {
                Monitor.Exit(_oneSyncAtATimeLock);
            }
        }
    }
}
