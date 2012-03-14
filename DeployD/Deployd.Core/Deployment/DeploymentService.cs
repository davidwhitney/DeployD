using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Deployd.Core.Caching;
using Deployd.Core.Deployment.Hooks;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using NuGet;
using log4net;

namespace Deployd.Core.Deployment
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IEnumerable<IDeploymentHook> _hooks;
        private readonly INuGetPackageCache _packageCache;
        private readonly ICurrentInstalledCache _currentInstalledCache;
        protected static readonly ILog Logger = LogManager.GetLogger("DeploymentService"); 
        public ApplicationContext AppContext { get; set; }

        public DeploymentService(IEnumerable<IDeploymentHook> hooks, 
                                 INuGetPackageCache packageCache,
                                 ICurrentInstalledCache currentInstalledCache)
        {
            _hooks = hooks;
            _packageCache = packageCache;
            _currentInstalledCache = currentInstalledCache;
        }

        /*
         * TODO: move this to the cache service
        public IList<LocalPackageInformation> AvailablePackages()
        {
            var packageDetails = new List<LocalPackageInformation>();
            foreach (var packageId in _packageCache.AvailablePackages)
            {
                var packageViewModel = new LocalPackageInformation { PackageId = packageId };
                var installed = _currentInstalledCache.GetCurrentInstalledVersion(packageId);
                if (installed != null)
                {
                    packageViewModel.InstalledVersion = installed.Version.Version.ToString();
                }
                var latestAvailable = _packageCache.GetLatestVersion(packageId);
                if (latestAvailable != null)
                {
                    packageViewModel.LatestAvailableVersion = latestAvailable.Version.Version.ToString();
                }
                packageDetails.Add(packageViewModel);
            }

            return packageDetails;
        }*/

        public void InstallPackage(string packageId, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            InstallPackage(packageId, () => _packageCache.GetLatestVersion(packageId), cancellationToken, reportProgress, taskId);
        }

        public void InstallPackage(string packageId, string specificVersion, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            var packageSelector = specificVersion == null
                                       ? (Func<IPackage>) (() => _packageCache.GetLatestVersion(packageId))
                                       : (() => _packageCache.GetSpecificVersion(packageId, specificVersion));

            InstallPackage(packageId, packageSelector, cancellationToken, reportProgress, taskId);
        }

        public void Deploy(string taskId, IPackage package, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            var frameworks = package.GetSupportedFrameworks();
            foreach(var framework in frameworks)
            {
                Logger.DebugFormat("package supports {0}", framework.FullName);
            }

            var outputPath = @"d:\temp\" + package.GetFullName();
            
            try
            {
                reportProgress(ProgressReport.Info(package.Id, package.Version.Version.ToString(), taskId, "Extracting package to temp folder..."));
                new PackageExtractor().Extract(package, outputPath);
            } 
            catch (Exception ex)
            {
                Logger.Fatal("Could not extract package", ex);
            }

            var targetInstallationFolder = Path.Combine(@"d:\wwwcom", package.Id);
            var deploymentContext = new DeploymentContext(package, outputPath, targetInstallationFolder, taskId);
            
            BeforeDeploy(deploymentContext, reportProgress);
            PerformDeploy(deploymentContext, reportProgress);
            AfterDeploy(deploymentContext, reportProgress);

            reportProgress(ProgressReport.Info(package.Id, package.Version.Version.ToString(), taskId,
                                               "Deployment complete"));
        }

        public void InstallPackage(string packageId, Func<IPackage> selectionCriteria, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress, string taskId)
        {
            var package = selectionCriteria();
            Deploy(taskId, package, cancellationToken, reportProgress);
            WriteInstallMarker(package);
        }

        private void WriteInstallMarker(IPackage package)
        {
            try
            {
                _currentInstalledCache.SetCurrentInstalledVersion(package);
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not set current installed version", ex);
            }
        }

        protected virtual void BeforeDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ForEachHook(context, "BeforeDeploy", hook => hook.BeforeDeploy(context), reportProgress);
        }

        protected virtual void AfterDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ForEachHook(context, "AfterDeploy", hook => hook.AfterDeploy(context), reportProgress);
        }

        protected virtual void PerformDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ForEachHook(context, "PerformDeploy", hook => hook.Deploy(context), reportProgress);
        }
        
        private void ForEachHook(DeploymentContext context, string comment, Action<IDeploymentHook> action, Action<ProgressReport> reportProgress)
        {
            foreach (var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
                    reportProgress(ProgressReport.InfoFormat(context.Package.Id, context.Package.Version.Version.ToString(), context.InstallationTaskId, "Running {0} hook {1}...", comment, hook.GetType().Name));
                    action(hook);
                }
                else
                {
                    Logger.DebugFormat("Skipping {0} for {1}", comment, hook.GetType());
                }
            }
        }
    }
}
