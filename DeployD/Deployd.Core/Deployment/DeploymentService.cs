using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Caching;
using Deployd.Core.Deployment.Hooks;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using NuGet;
using log4net;
using log4net.Repository;

namespace Deployd.Core.Deployment
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IEnumerable<IDeploymentHook> _hooks;
        private readonly INuGetPackageCache _packageCache;
        private readonly ICurrentInstalledCache _currentInstalledCache;
        private readonly IAgentSettings _agentSettings;
        protected static readonly ILog Logger = LogManager.GetLogger("DeploymentService"); 
        public ApplicationContext AppContext { get; set; }

        public DeploymentService(IEnumerable<IDeploymentHook> hooks, 
                                 INuGetPackageCache packageCache,
                                 ICurrentInstalledCache currentInstalledCache,
            IAgentSettings agentSettings)
        {
            _hooks = hooks;
            _packageCache = packageCache;
            _currentInstalledCache = currentInstalledCache;
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

        public void Deploy(string taskId, IPackage package, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress)
        {
            var unpackFolder = Path.Combine(AgentSettings.AgentProgramDataPath, _agentSettings.UnpackingLocation);
            var workingFolder = Path.Combine(unpackFolder, package.GetFullName());
            var targetInstallationFolder = Path.Combine(@"d:\wwwcom", package.Id);
            var deploymentContext = new DeploymentContext(package, workingFolder, targetInstallationFolder, taskId);

            var logger = deploymentContext.GetLoggerFor(this);
            var frameworks = package.GetSupportedFrameworks();
            foreach(var framework in frameworks)
            {
                logger.DebugFormat("package supports {0}", framework.FullName);
            }

            
            try
            {
                reportProgress(ProgressReport.Info(deploymentContext, package.Id, package.Version.Version.ToString(), taskId, "Extracting package to temp folder..."));
                new PackageExtractor().Extract(package, workingFolder);
            } 
            catch (Exception ex)
            {
                logger.Fatal("Could not extract package", ex);
            }

            
            BeforeDeploy(deploymentContext, reportProgress);
            PerformDeploy(deploymentContext, reportProgress);
            AfterDeploy(deploymentContext, reportProgress);

            reportProgress(ProgressReport.Info(deploymentContext, package.Id, package.Version.Version.ToString(), taskId,
                                               "Deployment complete"));

            deploymentContext.RemoveAppender();
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
            var installationLogger = context.GetLoggerFor(this);
            foreach (var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
                    reportProgress(ProgressReport.InfoFormat(context, context.Package.Id, context.Package.Version.Version.ToString(), context.InstallationTaskId, "Running {0} hook {1}...", comment, hook.GetType().Name));

                    try
                    {
                        action(hook);
                    } catch (Exception ex)
                    {
                        installationLogger.Fatal("Errors encountered, will not continue deploying", ex);
                        break;
                    }
                }
                else
                {
                    installationLogger.DebugFormat("Skipping {0} for {1}", comment, hook.GetType());
                }
            }
        }
    }
}
