using System;
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
            while (PendingInstalls.Count > 0)
            {
                var nextPendingInstall = PendingInstalls.Dequeue();
                RunningInstalls.Add(nextPendingInstall);
                
                nextPendingInstall.Task = new Task<InstallationResult>(() =>
                {
                    Logger.Info("Task started.");

                    if (string.IsNullOrWhiteSpace(nextPendingInstall.Version))
                    {
                        _deploymentService.InstallPackage(nextPendingInstall.PackageId,
                                                          Guid.NewGuid().ToString(), new CancellationTokenSource(),
                                                          progressReport => nextPendingInstall.ProgressReports.Add(progressReport));

                    }
                    else
                    {
                        _deploymentService.InstallPackage(nextPendingInstall.PackageId, nextPendingInstall.Version,
                                                          Guid.NewGuid().ToString(), new CancellationTokenSource(),
                                                          progressReport => nextPendingInstall.ProgressReports.Add(progressReport));
                    }

                    return new InstallationResult();
                });

                nextPendingInstall.Task.ContinueWith(RemoveFromRunningInstallationList);

                nextPendingInstall.Task.Start();
            }
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
