using System.Collections.Generic;

namespace Deployd.Agent.WebUi.Models
{
    public class AgentStatusViewModel
    {
        public IList<LocalPackageInformation> Packages { get; set; }
        public IList<InstallTaskViewModel> CurrentTasks { get; set; }
        public IEnumerable<string> AvailableVersions { get; set; }
        public string Environment { get; set; }
        public bool Contacted { get; set; }

        public AgentStatusViewModel()
        {
            Packages = new List<LocalPackageInformation>();
        }
    }
}