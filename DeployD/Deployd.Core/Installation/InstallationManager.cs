using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Deployd.Core.Installation
{
    public class InstallationManager : IInstallationManager
    {
        private readonly ILog _logger = LogManager.GetLogger("InstallationManager");
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

        public InstallationManager(IDeploymentService deploymentService)
        {
            _deploymentService = deploymentService;
            _progressReportAction = ReportProgress;
        }

        public void StartInstall(string packageId, string version = null)
        {
            var cancellationToken = new CancellationTokenSource();
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskFactory<InstallationResult>().StartNew(() =>
                    {
                        _deploymentService.InstallPackage(packageId, version, taskId, cancellationToken, _progressReportAction);
                        return new InstallationResult();
                    }
                );

            task.Start();

            InstallationTasks.Add(new InstallationTask(packageId, version, taskId, task, cancellationToken));
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