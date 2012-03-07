using System;
using System.Collections.Generic;
using System.IO;
using Deployd.Agent.Services.Deployment.Hooks;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using NuGet;
using log4net;

namespace Deployd.Agent.Services.Deployment
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IEnumerable<IDeploymentHook> _hooks;
        private readonly INuGetPackageCache _packageCache;
        private readonly ICurrentInstalledCache _currentInstalledCache;
        protected static readonly ILog Logger = LogManager.GetLogger("DeploymentService"); 
        public ApplicationContext AppContext { get; set; }

        public DeploymentService(IEnumerable<IDeploymentHook> hooks, INuGetPackageCache packageCache, ICurrentInstalledCache currentInstalledCache)
        {
            _hooks = hooks;
            _packageCache = packageCache;
            _currentInstalledCache = currentInstalledCache;
        }

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
        }

        public void InstallPackage(string packageId)
        {
            InstallPackage(packageId, () => _packageCache.GetLatestVersion(packageId));
        }

        public void InstallPackage(string packageId, string specificVersion)
        {
            InstallPackage(packageId, () => _packageCache.GetSpecificVersion(packageId, specificVersion));
        }

        public void Deploy(IPackage package)
        {
            var frameworks = package.GetSupportedFrameworks();
            foreach(var framework in frameworks)
            {
                Logger.DebugFormat("package supports {0}", framework.FullName);
            }

            var outputPath = @"d:\temp\" + package.GetFullName();
            
            try
            {
                new PackageExtractor().Extract(package, outputPath);
            } 
            catch (Exception ex)
            {
                Logger.Fatal("Could not extract package", ex);
            }

            var targetInstallationFolder = Path.Combine(@"d:\wwwcom", package.Id);
            var deploymentContext = new DeploymentContext(package, outputPath, targetInstallationFolder);
            
            BeforeDeploy(deploymentContext);
            PerformDeploy(deploymentContext);
            AfterDeploy(deploymentContext);
        }
        
        public void InstallPackage(string packageId, Func<IPackage> selectionCriteria)
        {
            var package = selectionCriteria();
            Deploy(package);
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

        protected virtual void BeforeDeploy(DeploymentContext context)
        {
            ForEachHook(context, "BeforeDeploy", hook => hook.BeforeDeploy(context));
        }

        protected virtual void AfterDeploy(DeploymentContext context)
        {
            ForEachHook(context, "AfterDeploy", hook => hook.AfterDeploy(context));
        }

        protected virtual void PerformDeploy(DeploymentContext context)
        {
            ForEachHook(context, "PerformDepoy", hook => hook.Deploy(context));
        }
        
        private void ForEachHook(DeploymentContext context, string comment, Action<IDeploymentHook> action)
        {
            foreach (var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
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
