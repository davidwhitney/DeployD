using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Installation.Hooks;
using Deployd.Core.PackageCaching;
using Deployd.Core.PackageFormats.NuGet;
using NuGet;
using log4net;

namespace Deployd.Core.Installation
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IEnumerable<IDeploymentHook> _hooks;
        private readonly ILocalPackageCache _packageCache;
        private readonly IInstalledPackageArchive _installedPackageArchive;
        private readonly IAgentSettings _agentSettings;
        protected static readonly ILog Logger = LogManager.GetLogger("DeploymentService"); 
        public ApplicationContext AppContext { get; set; }

        public DeploymentService(IEnumerable<IDeploymentHook> hooks, 
                                 ILocalPackageCache packageCache,
                                 IInstalledPackageArchive installedPackageArchive,
            IAgentSettings agentSettings)
        {
            _hooks = hooks;
            _packageCache = packageCache;
            _installedPackageArchive = installedPackageArchive;
            _agentSettings = agentSettings;
        }

        public void InstallPackage(string packageId, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            InstallPackage(packageId, () => _packageCache.GetLatestVersion(packageId), cancellationToken, reportProgress, taskId);
        }

        public void InstallPackage(string packageId, string specificVersion, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            var packageSelector = specificVersion == null || specificVersion == "latest"
                                       ? (Func<IPackage>) (() => _packageCache.GetLatestVersion(packageId))
                                       : (() => _packageCache.GetSpecificVersion(packageId, specificVersion));

            InstallPackage(packageId, packageSelector, cancellationToken, reportProgress, taskId);
        }

        public bool Deploy(string taskId, IPackage package, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            var unpackFolder = Path.Combine(AgentSettings.AgentProgramDataPath, _agentSettings.UnpackingLocation);
            var workingFolder = Path.Combine(unpackFolder, package.GetFullName());
            var targetInstallationFolder = Path.Combine(_agentSettings.BaseInstallationPath, package.Id);
            var deploymentContext = new DeploymentContext(package, _agentSettings, workingFolder, targetInstallationFolder, taskId);

            var logger = deploymentContext.GetLoggerFor(this);
            var frameworks = package.GetSupportedFrameworks();
            foreach(var framework in frameworks)
            {
                logger.DebugFormat("package supports {0}", framework.FullName);
            }

            
            try
            {
                reportProgress(ProgressReport.Info(deploymentContext, this, package.Id, package.Version.Version.ToString(), taskId, "Extracting package to temp folder..."));
                new NuGetPackageExtractor().Extract(package, workingFolder);
            } 
            catch (Exception ex)
            {
                logger.Fatal("Could not extract package", ex);
            }

            try
            {
                BeforeDeploy(deploymentContext, reportProgress);
                PerformDeploy(deploymentContext, reportProgress);
                AfterDeploy(deploymentContext, reportProgress);

                reportProgress(ProgressReport.Info(deploymentContext, this, package.Id, package.Version.Version.ToString(), taskId,
                                       "Deployment complete"));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred", ex);
                reportProgress(ProgressReport.Error(deploymentContext, this, package.Id, package.Version.Version.ToString(), taskId,
                                       "Deployment failed", ex));
                return false;
            }
            finally
            {
                deploymentContext.RemoveAppender();
            }
        }

        public void InstallPackage(string packageId, Func<IPackage> selectionCriteria, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress, string taskId)
        {
            var package = selectionCriteria();
            if (Deploy(taskId, package, cancellationToken, reportProgress))
            {
                WriteInstallMarker(package);
            }
        }

        private void WriteInstallMarker(IPackage package)
        {
            try
            {
                _installedPackageArchive.SetCurrentInstalledVersion(package);
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
            var installationLogger = context.GetLoggerFor(this);
            foreach (var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
                    reportProgress(ProgressReport.InfoFormat(context, this, context.Package.Id, context.Package.Version.Version.ToString(), context.InstallationTaskId, "Running {0} hook {1}...", comment, hook.GetType().Name));

                    action(hook);
                }
                else
                {
                    installationLogger.DebugFormat("Skipping {0} for {1}", comment, hook.GetType());
                }
            }
        }
    }
}
