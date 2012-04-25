using System;
using System.Collections.Generic;

namespace Deployd.Core.AgentManagement
{
    public class PendingActionsQueue : Queue<ActionTask>
    {
        public ActionTask Add(string packageId, string action, string scriptPath)
        {
            var actionTask = new ActionTask(packageId, action, scriptPath);
            actionTask.DateQueued = DateTime.Now;
            Enqueue(actionTask);
            return actionTask;
        }
    }

    public class CompletedActionsList : List<ActionTask>
    {
        public new void Add(ActionTask task)
        {
            task.DateCompleted = DateTime.Now;
            task.State = "Completed";
            base.Add(task);
        }
    }

    public class RunningActionsList : List<ActionTask>
    {
        public new void Add(ActionTask task)
        {
            task.DateStarted = DateTime.Now;
            task.State = "Running";
            base.Add(task);
        }
    }
}