using System.Collections.Generic;

namespace DeployD.Hub.Areas.Api.Models.Dto
{
    public class AgentStatusReport
    {
        public List<PackageViewModel> packages { get; set; }

        public List<InstallTaskViewModel> currentTasks { get; set; }

        public List<string> availableVersions { get; set; }

        public string environment { get; set; }
    }
}