using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Deployd.Core.AgentManagement
{
    public class ActionTask
    {
        public ActionTask(string packageId, string action, string scriptPath)
        {
            PackageId = packageId;
            ScriptName = action;
            ScriptPath = scriptPath;
            DateQueued = DateTime.Now;
            Id = Guid.NewGuid().ToString();
            State = "Pending";
        }

        public string Id { get; set; }

        public Task<ActionTaskResult> Task { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }
        public string PackageId { get; set; }
        public string ScriptName { get; set; }
        public List<Exception> Errors { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateQueued { get; set; }
        public DateTime DateCompleted { get; set; }
        public string Log { get; set; }

        public Exception Exception { get; set; }

        public string ScriptPath { get; set; }

        public string State { get; set; }

    }
}