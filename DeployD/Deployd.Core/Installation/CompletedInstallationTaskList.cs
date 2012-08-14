using System.Collections.Generic;
using log4net;

namespace Deployd.Core.Installation
{
    public class CompletedInstallationTaskList : List<InstallationTask>
    {
        private static ILog _logger = LogManager.GetLogger(typeof (CompletedInstallationTaskList));
        public new void Add(InstallationTask task)
        {
            _logger.InfoFormat("Disposing of task {0}", task.Task.Id);
            task.Task.Dispose();
            task.Task = null;
            task.ProgressReports.Clear();
            
        }
    }
}