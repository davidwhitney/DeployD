using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deployd.Core;
using Deployd.Core.Deployment;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using log4net;

namespace Deployd.Agent.Services.InstallationService
{
    public class PackageInstallationService : IWindowsService
    {
        private readonly IDeploymentService _deploymentService;
        protected static readonly ILog Logger = LogManager.GetLogger("PackageInstallationService");

        public ApplicationContext AppContext { get; set; }
        public TimedSingleExecutionTask TimedTask { get; private set; }

        public InstallationTaskQueue PendingInstalls { get; set; }
        public RunningInstallationTaskList RunningInstalls { get; set; }

        public PackageInstallationService(InstallationTaskQueue pendingInstalls, RunningInstallationTaskList runningInstalls, IDeploymentService deploymentService)
        {
            _deploymentService = deploymentService;
            PendingInstalls = pendingInstalls;
            RunningInstalls = runningInstalls;
            TimedTask = new TimedSingleExecutionTask(5000, CheckForNewInstallations);
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
                
                if (InstallationIsAlreadyRunningFor(nextPendingInstall.PackageId, nextPendingInstall.Version))
                {
                    alreadyRunning.Add(nextPendingInstall);
                    continue;
                }
                
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
                                                    progressReport =>
                                                        {
                                                            log4net.Core.Level level = log4net.Core.Level.Info;
                                                            switch(progressReport.Level)
                                                            {
                                                                case "Debug":
                                                                    level = log4net.Core.Level.Debug;
                                                                    break;
                                                                case "Warn":
                                                                    level = log4net.Core.Level.Warn;
                                                                    break;
                                                                case "Error":
                                                                    level = log4net.Core.Level.Error;
                                                                    break;
                                                                case "Fatal":
                                                                    level = log4net.Core.Level.Fatal;
                                                                    break;
                                                                case "Info":
                                                                default:
                                                                    level = log4net.Core.Level.Info;
                                                                    break;
                                                            }

                                                            progressReport.Context.GetLoggerFor(this).Logger.Log(
                                                                progressReport.ReportingType,
                                                                level,
                                                                progressReport.Message,
                                                                progressReport.Exception);

                                                            nextPendingInstall.ProgressReports.Add(progressReport);
                                                        });
                return new InstallationResult();
            });

            nextPendingInstall.Task.ContinueWith(RemoveFromRunningInstallationList);

            nextPendingInstall.Task.Start();
        }

        private void RemoveFromRunningInstallationList(Task<InstallationResult> completedInstallationTask)
        {
            var installationTask =
                RunningInstalls.SingleOrDefault(install => install.Task.Id == completedInstallationTask.Id);
            if (installationTask != null)
            {
                RunningInstalls.Remove(installationTask);
            }
        }
    }
}
