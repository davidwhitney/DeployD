using System.Collections.Generic;

namespace DeployD.Hub.Areas.Api.Models
{
    public class AgentRecord
    {
        public string Id { get; set; }
        public string Hostname { get; set; }
        public List<PackageViewModel> Packages { get; set; }
        public List<string> AvailableVersions { get; set; }
        public List<InstallTaskViewModel> CurrentTasks { get; set; }
        public string Environment { get; set; }
        public bool Contacted { get; set; }
    }
}