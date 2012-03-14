using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Caching;
using Deployd.Core.Deployment;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using MS.Internal.Xml.XPath;
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
            
            Get["/packages"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var installationManager = Container().GetType<IInstallationManager>();
                return View["packages.cshtml", 
                    new PackageListViewModel
                        {
                            Packages = cache.AvailablePackages.Select(name=>new LocalPackageInformation(){PackageId=name}).ToArray(),
                            CurrentTasks = installationManager.GetAllTasks()
                            .Select(t=>new InstallTaskViewModel()
                            {
                                Messages = t.ProgressReports.Select(pr=>pr.Message).ToArray(), 
                                Status=Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                PackageId=t.PackageId,
                                Version=t.Version,
                                LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                            }).ToList()
                        }];
            };

            Get["/installations"] = x =>
            {
                var taskQueue = Container().GetType<InstallationTaskQueue>();
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                var viewModel = new InstallationsViewModel {TaskQueue = taskQueue, RunningTasks = runningTasks};
                return View["installations.cshtml", viewModel];
            };

            Get["/packages/{packageId}"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var packageVersions = cache.AvailablePackageVersions(x.packageId);
                return View["package-details.cshtml", new PackageVersionsViewModel(x.packageId, packageVersions)];
            };

            Post["/packages/{packageId}/install", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                installationManager.Add(x.packageId);
                return Response.AsRedirect("/packages");
            };

            Post["/packages/{packageId}/install/{specificVersion}", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                installationManager.Add(x.packageId, x.specificVersion);
                return Response.AsRedirect("/packages");
            };
        }

        private void ReportProgress(ProgressReport progressReport)
        {
            _log.Info("Installer report: " + progressReport.Message);
        }
    }
}
