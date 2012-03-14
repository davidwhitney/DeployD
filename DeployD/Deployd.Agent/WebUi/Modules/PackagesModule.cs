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
    public class PackagesModule : NancyModule
    {
        private ILog _log = LogManager.GetLogger("PackagesModule");
        public static Func<IIocContainer> Container { get; set; }
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();

        public PackagesModule(): base("/packages")
        {
            Get["/"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var installationManager = Container().GetType<IInstallationManager>();
                var model =
                            new PackageListViewModel
                                {
                                    Packages = cache.AvailablePackages.Select(name => new LocalPackageInformation() { PackageId = name }).ToArray(),
                                    CurrentTasks = installationManager.GetAllTasks()
                                        .Select(t => new InstallTaskViewModel()
                                                        {
                                                            Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                            Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                            PackageId = t.PackageId,
                                                            Version = t.Version,
                                                            LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                                                        }).ToList()
                                };


                return this.ViewOrJson("packages.cshtml", model);
            };


            Get["/{packageId}"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var packageVersions = cache.AvailablePackageVersions(x.packageId);

                return this.ViewOrJson("package-details.cshtml", new PackageVersionsViewModel(x.packageId, packageVersions));
            };

            Post["/{packageId}/install", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                installationManager.Add(x.packageId);
                return Response.AsRedirect("/packages");
            };

            Post["/{packageId}/install/{specificVersion}", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                installationManager.Add(x.packageId, x.specificVersion);
                return Response.AsRedirect("/packages");
            };
        }
    }
}