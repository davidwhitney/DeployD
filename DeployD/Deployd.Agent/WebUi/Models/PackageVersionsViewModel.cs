using System.Collections.Generic;
using Deployd.Core.AgentManagement;
using Deployd.Core.Installation;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageVersionsViewModel
    {
        public string PackageName { get; set; }
        public string CurrentInstalledVersion { get; set; }
        public InstallationTask CurrentInstallTask { get; set; }
        public IEnumerable<AgentAction> Actions { get; set; }
        public List<string> CachedVersions { get; set; }

        public PackageVersionsViewModel(string packageName, IEnumerable<string> inner, string currentInstalledVersion, InstallationTask currentInstallTask,
            IEnumerable<AgentAction> actions )
        {
            PackageName = packageName;
            CurrentInstalledVersion = currentInstalledVersion;
            CurrentInstallTask = currentInstallTask;
            Actions = actions;
            CachedVersions = new List<string>(inner);
        }
    }
}