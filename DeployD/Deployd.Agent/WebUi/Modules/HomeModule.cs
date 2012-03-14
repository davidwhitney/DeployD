using System;
using System.Collections.Generic;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Nancy;
using log4net;

namespace Deployd.Agent.WebUi.Modules
{
    public class HomeModule : NancyModule
    {
        private ILog _log = LogManager.GetLogger("HomeModule");
        public static Func<IIocContainer> Container { get; set; }
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();

        public HomeModule()
        {
            Get["/"] = x => View["index.cshtml"];
        }
    }
}
