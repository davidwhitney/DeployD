using System;
using System.Collections.Generic;
using System.Linq;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Nancy;

namespace Deployd.Agent.WebUi.Modules
{
    public class InstallationsModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();
        
        public InstallationsModule() : base("/installations")
        {
            Get["/"] = x =>
            {
                var taskQueue = Container().GetType<InstallationTaskQueue>();
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                return this.ViewOrJson("installations.cshtml", new InstallationsViewModel {TaskQueue = taskQueue, Tasks = runningTasks.ToList()});
            };

            Get["/completed"] = x =>
            {
                var taskList = Container().GetType<CompletedInstallationTaskList>();
                var viewModel = new InstallationsViewModel {Tasks = taskList.ToList()};
                return this.ViewOrJson("installations/completed.cshtml", viewModel);
            };
        }
    }
}