using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deployd.Core;
using Deployd.Core.AgentManagement;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;

using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.Services
{
    public class ActionExecutionService : IWindowsService
    {
        private readonly ILogger _logger;
        public PendingActionsQueue PendingActionsQueue { get; set; }
        public RunningActionsList RunningActionsList { get; set; }
        public CompletedActionsList CompletedActionsList { get; set; }
        public IAgentActionsService ActionsService { get; set; }
        private IAgentActionsRepository _actionsRepository;
        
        private TimedSingleExecutionTask TimedTask { get; set; }
        public ActionExecutionService(PendingActionsQueue pendingActionsQueue, 
            CompletedActionsList completedActionsList,
            RunningActionsList runningActionsList,
            IAgentActionsService actionsService, IAgentActionsRepository actionsRepository,
            ILogger logger)
        {
            _logger = logger;
            PendingActionsQueue = pendingActionsQueue;
            CompletedActionsList = completedActionsList;
            RunningActionsList = runningActionsList;
            ActionsService = actionsService;
            _actionsRepository = actionsRepository;
            TimedTask = new TimedSingleExecutionTask(2000, CheckForPendingActions, _logger);
        }

        public ActionTask GetAction(string id)
        {
            ActionTask action = null;
            action = PendingActionsQueue.SingleOrDefault(a=>a.Id==id);
            if (action == null)
                action = RunningActionsList.SingleOrDefault(a => a.Id == id);
            if (action == null)
                action = CompletedActionsList.SingleOrDefault(a => a.Id == id);

            return action;
        }

        private void CheckForPendingActions()
        {
            if (PendingActionsQueue.Count > 0)
            {
                var action = PendingActionsQueue.Dequeue();
                RunningActionsList.Add(action);
                StartAction(action);
            }
        }

        private void StartAction(ActionTask action)
        {
            action.Task= new Task<ActionTaskResult>(() =>
            {
                ActionsService.RunAction(action,
                    progressReport=>HandleProgressReport(action,progressReport));
                return new ActionTaskResult();
            });
            action.Task
                .ContinueWith(RemoveFromRunningActions)
                .ContinueWith(task => _logger.Error(action.Exception, "Action failed"),
                              TaskContinuationOptions.OnlyOnFaulted);
            action.Task.Start();
        }

        private void HandleProgressReport(ActionTask action, ProgressReport progressReport)
        {
            action.Log += progressReport.Message + "\r\n";
        }

        private void RemoveFromRunningActions(Task<ActionTaskResult> completedActionTask)
        {
            var runningAction = RunningActionsList.SingleOrDefault(action => action.Task.Id == completedActionTask.Id);
            if (runningAction!= null)
            {
                RunningActionsList.Remove(runningAction);
            }
            CompletedActionsList.Add(runningAction);
        }


        public void Start(string[] args)
        {
            TimedTask.Start(args);
        }

        public void Stop()
        {
            TimedTask.Stop();
        }

        public ApplicationContext AppContext { get; set; }
    }
}
