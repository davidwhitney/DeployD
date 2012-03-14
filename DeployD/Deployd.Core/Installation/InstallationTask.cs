using System.Collections.Generic;
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
        }

        public Task<InstallationResult> Task { get; private set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }
        public string InstallationTaskId { get; private set; }
        public string PackageId { get; private set; }
        public string Version { get; private set; }
        public List<ProgressReport> ProgressReports { get; private set; }
    }
}