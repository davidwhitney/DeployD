using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployD.Hub.Areas.Api.Code;

namespace DeployD.Hub.Areas.Api.Models
{
    public class AgentViewModel
    {
        public string id { get; set; }
        public IEnumerable<PackageViewModel> packages { get; set; }
    }
}