using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core.Installation
{
    public class InstallationManager : IInstallationManager
    {
        private readonly ILogger _logger;
        private readonly IDeploymentService _deploymentService;
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();

        private readonly Action<ProgressReport> _progressReportAction;

        private void ReportProgress(ProgressReport report)
        {
            var installationLogger = report.Context.GetLoggerFor(this);
            installationLogger.Info(report.Message);
            
            var task = GetTaskById(report.InstallationTaskId);
            if (task != null)
            {
                task.ProgressReports.Add(report);
            }
        }

        public InstallationManager(IDeploymentService deploymentService, ILogger logger)
        {
            _deploymentService = deploymentService;
            _logger = logger;
            _progressReportAction = ReportProgress;
        }

        public void StartInstall(string packageId, string version = null)
        {
            var cancellationToken = new CancellationTokenSource();
            string taskId = Guid.NewGuid().ToString();
            
            var task = new TaskFactory<InstallationResult>()
                .StartNew(() => _deploymentService.InstallPackage(packageId, version, taskId, cancellationToken, _progressReportAction)); // set the result on the installation object
            InstallationTasks.Add(new InstallationTask(packageId, version, taskId, task, cancellationToken));
            task.Start();
        }

        public void ClearCompletedTasks()
        {
            foreach(var task in InstallationTasks)
            {
                if (task.Task.IsCompleted || task.Task.IsCanceled || task.Task.IsFaulted)
                {
                    task.Task.Dispose();
                }
            }
        }

        public InstallationTask GetTaskById(string installationTaskId)
        {
            return InstallationTasks.SingleOrDefault(t => t.InstallationTaskId == installationTaskId);
        }

        public List<InstallationTask> GetAllTasks()
        {
            return InstallationTasks;
        }
    }
}