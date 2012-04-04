using System.Collections.Generic;
using Deployd.Core.Installation;

namespace Deployd.Agent.WebUi.Models
{
    public class InstallationsViewModel
    {
        public InstallationTaskQueue TaskQueue { get; set; }
        public List<InstallationTask> Tasks { get; set; }
    }
}
