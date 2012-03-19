using System.Collections.Generic;

namespace DeployD.Hub.Areas.Api.Models
{
    public class AgentViewModel
    {
        public string hostname { get; set; }
        public IEnumerable<PackageViewModel> packages { get; set; }
    }
}