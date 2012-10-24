using System.Collections.Generic;
using Deployd.Core.AgentManagement;

namespace Deployd.Agent.WebUi.Modules
{
    public class ActionDetailsViewModel
    {
        public IEnumerable<ActionTask> Pending { get; set; }

        public IEnumerable<ActionTask> Running { get; set; }

        public IEnumerable<ActionTask> Completed { get; set; }

        public string PackageId { get; set; }

        public string Action { get; set; }
    }
}