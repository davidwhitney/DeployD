using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deployd.Agent.Services.HubCommunication;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using Deployd.Core.Remoting;
using log4net.Core;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.Services.InstallationService
{
    public class PackageInstallationService : IWindowsService
    {
        private readonly IDeploymentService _deploymentService;
        private readonly ILogger _logger;
        private readonly IHubCommunicator _hubCommunicator;
        private ILocalPackageCache _agentCache;
        private IInstalledPackageArchive _installCache;
        private RunningInstallationTaskList _runningTasks;
        private IAgentSettingsManager _settingsManager;
        private readonly IPackagesList _allPackagesList;

        public ApplicationContext AppContext { get; set; }
        public TimedSingleExecutionTask TimedTask { get; private set; }

        public InstallationTaskQueue PendingInstalls { get; set; }
        public RunningInstallationTaskList RunningInstalls { get; set; }
        public CompletedInstallationTaskList CompletedInstalls { get; set; }

        public PackageInstallationService(InstallationTaskQueue pendingInstalls, 
            RunningInstallationTaskList runningInstalls, 
            CompletedInstallationTaskList completedInstalls,
            IDeploymentService deploymentService,
            ILogger logger,
            IHubCommunicator hubCommunicator, 
            ILocalPackageCache agentCache, 
            IInstalledPackageArchive installCache, 
            RunningInstallationTaskList runningTasks, 
            IAgentSettingsManager settingsManager,
            IPackagesList allPackagesList)
        {
            CompletedInstalls = completedInstalls;
            _deploymentService = deploymentService;
            _logger = logger;
            _hubCommunicator = hubCommunicator;
            _agentCache = agentCache;
            _installCache = installCache;
            _runningTasks = runningTasks;
            _settingsManager = settingsManager;
            _allPackagesList = allPackagesList;
            PendingInstalls = pendingInstalls;
            RunningInstalls = runningInstalls;
            TimedTask = new TimedSingleExecutionTask(5000, CheckForNewInstallations, _logger);
        }

        ~PackageInstallationService()
        {
            _logger.Warn("Destroying a {0}", this.GetType());

        }

        public void Start(string[] args)
        {
            TimedTask.Start(args);
        }

        public void Stop()
        {
            TimedTask.Stop();
        }

        public void CheckForNewInstallations()
        {
            var alreadyRunning = new List<InstallationTask>();

            while (PendingInstalls.Count > 0)
            {
                var nextPendingInstall = PendingInstalls.Dequeue();
                _logger.Debug("{0} {1} - Preparing installation", nextPendingInstall.PackageId, nextPendingInstall.Version);
                
                if (InstallationIsAlreadyRunningFor(nextPendingInstall.PackageId, nextPendingInstall.Version))
                {
                    _logger.Debug("{0} {1} - Already running, dropping", nextPendingInstall.PackageId, nextPendingInstall.Version);
                    alreadyRunning.Add(nextPendingInstall);
                    continue;
                }

                if (RunningInstalls.Count >= _settingsManager.Settings.MaxConcurrentInstallations)
                {
                    _logger.Debug("{0} {1} - Max installations already running, will try again soon", nextPendingInstall.PackageId, nextPendingInstall.Version);
                    alreadyRunning.Add(nextPendingInstall);
                    continue;
                }

                _logger.Debug("{0} {1} - Starting installation", nextPendingInstall.PackageId, nextPendingInstall.Version);
                RunningInstalls.Add(nextPendingInstall);
                StartInstall(nextPendingInstall);
            }

            ReQueueSkippedInstalls(alreadyRunning);
        }

        private void ReQueueSkippedInstalls(IEnumerable<InstallationTask> alreadyRunning)
        {
            foreach (var installationTask in alreadyRunning)
            {
                PendingInstalls.Enqueue(installationTask);
            }
        }

        private bool InstallationIsAlreadyRunningFor(string packageId, string version)
        {
            return RunningInstalls.Any(x => x.PackageId == packageId && x.Version == version);
        }

        private void StartInstall(InstallationTask nextPendingInstall)
        {
            nextPendingInstall.Task = new Task<InstallationResult>(() =>
            {
                _deploymentService.InstallPackage(nextPendingInstall.PackageId, nextPendingInstall.Version, Guid.NewGuid().ToString(), new CancellationTokenSource(),
                                                    progressReport => HandleProgressReport(nextPendingInstall, progressReport));
                return new InstallationResult();
            });

            nextPendingInstall.Task
                .ContinueWith(RemoveFromRunningInstallationList)
                .ContinueWith(task => _logger.Error(task.Exception, "Installation task failed."), TaskContinuationOptions.OnlyOnFaulted);
            _logger.Debug("Installation task queued");
            nextPendingInstall.Task.Start();
        }

        private void HandleProgressReport(InstallationTask installationTask, ProgressReport progressReport)
        {
            Level level;
            switch (progressReport.Level)
            {
                case "Debug":
                    level = Level.Debug;
                    break;
                case "Warn":
                    level = Level.Warn;
                    break;
                case "Error":
                    level = Level.Error;
                    break;
                case "Fatal":
                    level = Level.Fatal;
                    break;
                default:
                    level = Level.Info;
                    break;
            }

            progressReport.Context.GetLoggerFor(this).Logger.Log(
                progressReport.ReportingType,
                level,
                progressReport.Message,
                progressReport.Exception);

            installationTask.LogFileName = progressReport.Context.LogFileName;
            installationTask.ProgressReports.Add(progressReport);

            // update completed datetime in case an exception occurs
            installationTask.DateCompleted = DateTime.Now;

            if (progressReport.Exception == null)
            {
                _hubCommunicator.SendStatusToHubAsync(AgentStatusFactory.BuildStatus(_allPackagesList, _agentCache, _installCache, _runningTasks, _settingsManager));
                return;
            }

            installationTask.HasErrors = true;
            installationTask.Errors.Add(progressReport.Exception);

            _hubCommunicator.SendStatusToHubAsync(AgentStatusFactory.BuildStatus(_allPackagesList, _agentCache, _installCache, _runningTasks, _settingsManager));
        }

        private void RemoveFromRunningInstallationList(Task<InstallationResult> completedInstallationTask)
        {
            var installationTask = RunningInstalls.SingleOrDefault(install => install.Task.Id == completedInstallationTask.Id);
            if (installationTask != null)
            {
                RunningInstalls.Remove(installationTask);
            }
            CompletedInstalls.Add(installationTask);
        }
    }
}
