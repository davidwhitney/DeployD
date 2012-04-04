using System;
using System.Collections.Generic;
using System.Linq;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Nancy;
using log4net;

namespace Deployd.Agent.WebUi.Modules
{
    public class InstallationsModule : NancyModule
    {
        private ILog _log = LogManager.GetLogger("InstallationsModule");
        public static Func<IIocContainer> Container { get; set; }
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();
        
        public InstallationsModule() : base("/installations")
        {
            Get["/"] = x =>
            {
                var taskQueue = Container().GetType<InstallationTaskQueue>();
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                var viewModel = new InstallationsViewModel {TaskQueue = taskQueue, Tasks = runningTasks.ToList()};

                return this.ViewOrJson("installations.cshtml", viewModel);
            };

            Get["/completed"] = x =>
            {
                var taskList = Container().GetType<CompletedInstallationTaskList>();
                var viewModel = new InstallationsViewModel() {Tasks = taskList.ToList()};
                var response = this.ViewOrJson("installations/completed.cshtml", viewModel);
                return response;
            };
        }
    }
}