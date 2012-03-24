using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Caching;
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

            Get["/sitrep"] = (x) =>
                                 {
                                     _log.Info(string.Format("{0} asked for status", Request.UserHostAddress));
                                     var cache = Container().GetType<INuGetPackageCache>();
                                     var runningTasks = Container().GetType<RunningInstallationTaskList>();
                                     var installCache = Container().GetType<ICurrentInstalledCache>();

                                     var model =
                                                 new PackageListViewModel
                                                 {
                                                     Packages = cache.AvailablePackages.Select(name => new LocalPackageInformation()
                                                     {
                                                         PackageId = name,
                                                         InstalledVersion = installCache.GetCurrentInstalledVersion(name) != null ? installCache.GetCurrentInstalledVersion(name).Version.ToString() : "",
                                                         LatestAvailableVersion = cache.GetLatestVersion(name) != null ? cache.GetLatestVersion(name).Version.ToString() : "",
                                                         AvailableVersions = cache.AvailablePackageVersions(name).ToList(),
                                                         CurrentTask = runningTasks.Count > 0 ? runningTasks
                                                             .Where(t => t.PackageId == name)
                                                             .Select(t => new InstallTaskViewModel()
                                                             {
                                                                 Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                                 Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                                 PackageId = t.PackageId,
                                                                 Version = t.Version,
                                                                 LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                                                             }).FirstOrDefault()
                                                             : null
                                                     }).ToArray(),
                                                     CurrentTasks = runningTasks
                                                         .Select(t => new InstallTaskViewModel()
                                                         {
                                                             Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                             Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                             PackageId = t.PackageId,
                                                             Version = t.Version,
                                                             LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                                                         }).ToList(),
                                                     AvailableVersions = cache.AllCachedPackages().Select(p => p.Version.ToString()).Distinct().OrderByDescending(s => s)
                                                 };


                                     return this.ViewOrJson("sitrep.cshtml", model);
                                     
                                 };
        }
    }
}
