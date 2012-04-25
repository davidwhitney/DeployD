using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployD.Hub.Areas.Api.Code;

namespace DeployD.Hub.Areas.Api.Models
{
    public class AgentViewModel
    {
        public string id { get; set; }
        public List<PackageViewModel> packages { get; set; }
        public List<string> availableVersions { get; set; }
        public List<InstallTaskViewModel> currentTasks { get; set; }
        public string environment { get; set; }
        public bool contacted { get; set; }
        public bool Approved { get; set; }
    }
}