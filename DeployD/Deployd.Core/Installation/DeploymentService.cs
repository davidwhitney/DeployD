using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Installation.Hooks;
using Deployd.Core.Notifications;
using Deployd.Core.PackageCaching;
using Deployd.Core.PackageFormats.NuGet;
using Deployd.Core.PackageTransport;
using NuGet;
using log4net;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core.Installation
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IEnumerable<IDeploymentHook> _hooks;
        private readonly ILocalPackageCache _packageCache;
        private readonly IInstalledPackageArchive _installedPackageArchive;
        private readonly IAgentSettingsManager _agentSettingsManager;
        protected readonly ILogger Logger;
        private readonly IRetrievePackageQuery _nugetPackageQuery;
        private readonly INotificationService _notificationService;
        public ApplicationContext AppContext { get; set; }

        public DeploymentService(IEnumerable<IDeploymentHook> hooks, 
                                 ILocalPackageCache packageCache,
                                 IInstalledPackageArchive installedPackageArchive,
            IAgentSettingsManager agentSettingsManager, ILogger logger, IRetrievePackageQuery nugetPackageQuery,
            INotificationService notificationService)
        {
            _hooks = hooks;
            _packageCache = packageCache;
            _installedPackageArchive = installedPackageArchive;
            _agentSettingsManager = agentSettingsManager;
            Logger = logger;
            _nugetPackageQuery = nugetPackageQuery;
            _notificationService = notificationService;
        }

        public InstallationResult InstallPackage(string packageId, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            return InstallPackage(packageId, () => _packageCache.GetLatestVersion(packageId), cancellationToken, reportProgress, taskId);
        }

        public InstallationResult InstallPackage(string packageId, string specificVersion, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            var packageSelector = specificVersion == null || specificVersion == "latest"
                                       ? (Func<IPackage>) (() => _packageCache.GetLatestVersion(packageId))
                                       : (() => LoadOrDownloadSpecificPackageVersion(packageId, specificVersion));
            
            return InstallPackage(packageId, packageSelector, cancellationToken, reportProgress, taskId);
        }

        private IPackage LoadOrDownloadSpecificPackageVersion(string packageId, string specificVersion)
        {
            IPackage package = null;

            try
            {
                package = _packageCache.GetSpecificVersion(packageId, specificVersion);
            } catch (ArgumentOutOfRangeException)
            {
                package = _nugetPackageQuery.GetSpecificPackage(packageId, specificVersion);
                if (package == null)
                {
                    Logger.Debug("{0} Requested version {1} is not available at the given Nuget repository", packageId, specificVersion);
                    return null;
                }
                _packageCache.Add(package);
            }

            return package;
        }

        public bool Deploy(string taskId, IPackage package, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            var unpackFolder = Path.Combine(AgentSettings.AgentProgramDataPath, _agentSettingsManager.Settings.UnpackingLocation);
            var workingFolder = Path.Combine(unpackFolder, package.GetFullName());
            var targetInstallationFolder = Path.Combine(_agentSettingsManager.Settings.BaseInstallationPath, package.Id);
            using (var deploymentContext = new DeploymentContext(package, _agentSettingsManager, workingFolder, targetInstallationFolder, taskId))
            {

                var logger = deploymentContext.GetLoggerFor(this);
                var frameworks = package.GetSupportedFrameworks();
                foreach (var framework in frameworks)
                {
                    logger.DebugFormat("package supports {0}", framework.FullName);
                }


                try
                {
                    reportProgress(ProgressReport.Info(deploymentContext, this, package.Id,
                                                       package.Version.Version.ToString(), taskId,
                                                       "Extracting package to temp folder..."));
                    new NuGetPackageExtractor(Logger).Extract(package, workingFolder);
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

                    reportProgress(ProgressReport.Info(deploymentContext, this, package.Id,
                                                       package.Version.Version.ToString(), taskId,
                                                       "Deployment complete"));
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred", ex);
                    reportProgress(ProgressReport.Error(deploymentContext, this, package.Id,
                                                        package.Version.Version.ToString(), taskId,
                                                        "Deployment failed", ex));
                    return false;
                }
                finally
                {
                    deploymentContext.RemoveAppender();
                }
            }
        }

        public InstallationResult InstallPackage(string packageId, Func<IPackage> selectionCriteria, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress, string taskId)
        {
            var package = selectionCriteria();

            if (package ==null)
            {
                Logger.Debug("No package matching criteria could be found, aborting");

                return new InstallationResult(){Failed=true};
            }

            Logger.Debug("Installing {0} {1}", package.Title, package.Version);
            _notificationService.NotifyAll(EventType.Installation, string.Format("{0} {1} Installing", package.Title, package.Version));
            if (Deploy(taskId, package, cancellationToken, reportProgress))
            {
                WriteInstallMarker(package);

                _notificationService.NotifyAll(EventType.Installation, string.Format("{0} {1} installed successfully", package.Title, package.Version));

                return new InstallationResult() { Failed = false};
            }

            _notificationService.NotifyAll(EventType.Installation, string.Format("{0} {1} installation failed", package.Title, package.Version));
            return new InstallationResult(){Failed = true};
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
            ForEachHook(context, "BeforeDeploy", hook => hook.BeforeDeploy(context, reportProgress), reportProgress);
        }

        protected virtual void AfterDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ForEachHook(context, "AfterDeploy", hook => hook.AfterDeploy(context, reportProgress), reportProgress);
        }

        protected virtual void PerformDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ForEachHook(context, "PerformDeploy", hook => hook.Deploy(context, reportProgress), reportProgress);
        }
        
        private void ForEachHook(DeploymentContext context, string comment, Action<IDeploymentHook> action, Action<ProgressReport> reportProgress)
        {
            var installationLogger = context.GetLoggerFor(this);
            foreach (var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
                    reportProgress(ProgressReport.InfoFormat(context, this, context.Package.Id, context.Package.Version.Version.ToString(), context.InstallationTaskId, "Running {0} hook {1}...", comment, hook.ProgressMessage));

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
