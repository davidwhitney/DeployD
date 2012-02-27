using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Deployd.Core.Queries;
using log4net;
using Timer = System.Timers.Timer;

namespace Deployd.Agent.Services
{
    public abstract class PackageSyncServiceBase : IWindowsService
    {        
        protected static readonly ILog Logger = LogManager.GetLogger("PackageDownloadingService");
        
        public ApplicationContext AppContext { get; set; }

        private readonly IRetrieveAllAvailablePackageManifestsQuery _allPackagesQuery;
        private readonly INuGetPackageCache _agentCache;
        private readonly Timer _cacheUpdateTimer;
        private readonly object _oneSyncAtATimeLock;

        protected PackageSyncServiceBase(IRetrieveAllAvailablePackageManifestsQuery allPackagesQuery, INuGetPackageCache agentCache)
        {
            _allPackagesQuery = allPackagesQuery;
            _agentCache = agentCache;

            _cacheUpdateTimer = new Timer(60000) {Enabled = true};
            _oneSyncAtATimeLock = new object();
        }

        public void Start(string[] args)
        {
            _cacheUpdateTimer.Elapsed += LocallyCachePackages; 
            _cacheUpdateTimer.Start();
            LocallyCachePackages();
        }

        public void Stop()
        {
            _cacheUpdateTimer.Elapsed -= LocallyCachePackages; 
            _cacheUpdateTimer.Stop();
        }

        public void LocallyCachePackages(object sender, ElapsedEventArgs e)
        {
            LocallyCachePackages();
        }

        public void LocallyCachePackages()
        {
            if (!Monitor.TryEnter(_oneSyncAtATimeLock))
            {
                Logger.Info("Skipping a local cache operation because a previous cache operation is still running.");
                return;
            }
            
            try
            {
                foreach (var packageId in GetPackagesToDownload())
                {
                    _agentCache.Add(_allPackagesQuery.GetLatestPackage(packageId));
                }
            }
            finally
            {
                Monitor.Exit(_oneSyncAtATimeLock);
            }
        }

        public abstract IList<string> GetPackagesToDownload();
    }
}
