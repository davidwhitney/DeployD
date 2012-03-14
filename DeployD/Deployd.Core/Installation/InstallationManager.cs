using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deployd.Core.Deployment;
using log4net;

namespace Deployd.Core.Installation
{
    public interface IInstallationManager
    {
        void StartInstall(string packageId, string version);
        void StartInstall(string packageId);
        List<InstallationTask> GetAllTasks();
        InstallationTask GetTaskById(string installationTaskId);
    }

    public class InstallationManager : IInstallationManager
    {
        private ILog logger = LogManager.GetLogger("InstallationManager");
        private readonly IDeploymentService _deploymentService;
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();

        private readonly Action<ProgressReport> _progressReportAction;

        private void ReportProgress(ProgressReport report)
        {
            logger.Info(report.Message);

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

        public void StartInstall(string packageId)
        {
            var cancellationToken = new CancellationTokenSource();
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskFactory<InstallationResult>().StartNew(() =>
            {
                _deploymentService.InstallPackage(packageId,taskId, cancellationToken, _progressReportAction);
                return new InstallationResult();
            });

            InstallationTasks.Add(new InstallationTask(packageId, null, taskId, task, cancellationToken));
        }

        public void StartInstall(string packageId, string version)
        {
            var cancellationToken = new CancellationTokenSource();
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskFactory<InstallationResult>().StartNew(() =>
                    {
                        _deploymentService.InstallPackage(packageId, version, taskId, cancellationToken, _progressReportAction);
                        return new InstallationResult();
                    }
                );

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