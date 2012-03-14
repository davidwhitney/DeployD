using Deployd.Core.Installation;

namespace Deployd.Agent.WebUi.Models
{
    public class InstallationsViewModel
    {
        public InstallationTaskQueue TaskQueue { get; set; }
        public RunningInstallationTaskList RunningTasks { get; set; }
    }
}
