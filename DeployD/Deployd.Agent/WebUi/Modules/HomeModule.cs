using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Agent.WebUi.Models;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.AgentManagement;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using Nancy;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.WebUi.Modules
{
    public class HomeModule : NancyModule
    {
        private readonly IAgentSettings _agentSettings;
        private readonly ILogger _log;
        public static Func<IIocContainer> Container { get; set; }
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();

        public HomeModule()
        {
            _agentSettings = Container().GetType<IAgentSettings>();
            _log = Container().GetType<ILogger>();
            
            Get["/"] = x => View["index.cshtml"];

            Get["/sitrep"] = x =>
            {

                _log.Debug(string.Format("{0} asked for status", Request.UserHostAddress));
                var cache = Container().GetType<ILocalPackageCache>();
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                var installCache = Container().GetType<IInstalledPackageArchive>();

                var model =
                new AgentStatusViewModel
                {
                    Packages = cache.AvailablePackages.Select(name => new LocalPackageInformation()
                    {
                        PackageId = name,
                        InstalledVersion = installCache.GetCurrentInstalledVersion(name) != null ? installCache.GetCurrentInstalledVersion(name).Version.ToString() : "",
                        LatestAvailableVersion = cache.GetLatestVersion(name) != null ? cache.GetLatestVersion(name).Version.ToString() : "",
                        AvailableVersions = cache.AvailablePackageVersions(name).ToList(),
                        CurrentTask = runningTasks.Where(t => t.PackageId == name)
                            .Select(t => new InstallTaskViewModel()
                            {
                                Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                PackageId = t.PackageId,
                                Version = t.Version,
                                LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                            }).FirstOrDefault()
                    }).ToArray(),
                    CurrentTasks = runningTasks.Select(t => new InstallTaskViewModel()
                            {
                                Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                PackageId = t.PackageId,
                                Version = t.Version,
                                LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                            }).ToList(),
                    AvailableVersions = cache.AllCachedPackages().Select(p => p.Version.ToString()).Distinct().OrderByDescending(s => s),
                    Environment = _agentSettings.DeploymentEnvironment
                };


                return this.ViewOrJson("sitrep.cshtml", model);
                                     
            };
        }
    }
}
