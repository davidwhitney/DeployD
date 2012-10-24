using System.Collections.Generic;
using Deployd.Core.AgentManagement;

namespace Deployd.Agent.WebUi.Models
{
    public class ActionListViewModel
    {
        public List<AgentAction> Actions { get; set; }

        public string PackageId { get; set; }
    }
}