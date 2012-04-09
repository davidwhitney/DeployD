using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Deployd.Core.Installation
{
    public class InstallationTask
    {
        public InstallationTask(string packageId, string version, string taskId, Task<InstallationResult> task, CancellationTokenSource cancellationTokenSource)
        {
            InstallationTaskId = taskId;
            PackageId = packageId;
            Version = version;
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
            ProgressReports = new List<ProgressReport>();
            Errors = new List<Exception>();
        }

        public Task<InstallationResult> Task { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }
        public string InstallationTaskId { get; private set; }
        public string PackageId { get; private set; }
        public string Version { get; private set; }
        public List<ProgressReport> ProgressReports { get; private set; }
        public string LastMessage { get { return ProgressReports.Count > 0 ? ProgressReports.Last().Message : ""; } }

        public bool HasErrors { get; set; }

        public List<Exception> Errors { get; set; }

        public string LogFileName { get; set; }
    }
}