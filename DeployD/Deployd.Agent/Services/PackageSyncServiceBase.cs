using System;
using System.Collections.Generic;
using System.Linq;
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

        protected readonly IRetrieveAllAvailablePackageManifestsQuery _allPackagesQuery;
        protected readonly INuGetPackageCache _agentCache;
        private readonly Timer _cacheUpdateTimer;
        protected readonly object _oneSyncAtATimeLock;

        protected PackageSyncServiceBase(IRetrieveAllAvailablePackageManifestsQuery allPackagesQuery, INuGetPackageCache agentCache, int timerIntervalInMs)
        {
            _allPackagesQuery = allPackagesQuery;
            _agentCache = agentCache;

            _cacheUpdateTimer = new Timer(timerIntervalInMs) { Enabled = true };
            _oneSyncAtATimeLock = new object();
        }

        public void Start(string[] args)
        {
            _cacheUpdateTimer.Elapsed += FetchPackages; 
            _cacheUpdateTimer.Start();
            FetchPackages();
        }

        public void Stop()
        {
            _cacheUpdateTimer.Elapsed -= FetchPackages; 
            _cacheUpdateTimer.Stop();
        }

        public virtual void FetchPackages(object sender, ElapsedEventArgs e)
        {
            FetchPackages();
        }

        public virtual void FetchPackages()
        {
            OneAtATime(()=>
            {
                var packages = GetPackagesToDownload();
                foreach(var p in packages)
                {
                    Logger.Debug(p);
                }

                foreach (var latestPackageOfType in packages.Select(packageId => _allPackagesQuery.GetLatestPackage(packageId)))
                {
                    _agentCache.Add(latestPackageOfType);
                }                 
            });
        }

        protected void OneAtATime(Action action)
        {
            if (!Monitor.TryEnter(_oneSyncAtATimeLock))
            {
                Logger.Info("Skipping sync operation because a previous sync is still running.");
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                Monitor.Exit(_oneSyncAtATimeLock);
            }
        }

        public abstract IList<string> GetPackagesToDownload();
    }
}
