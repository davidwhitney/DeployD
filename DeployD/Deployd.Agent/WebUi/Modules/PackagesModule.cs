using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Nancy;
using NuGet;
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
                                                        AvailableVersions = cache.AllCachedPackages().Select(p=>p.Version.ToString()).Distinct().OrderByDescending(s=>s)
                                };


                return this.ViewOrJson("packages.cshtml", model);
            };


            Get["/{packageId}"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var packageVersions = cache.AvailablePackageVersions(x.packageId);
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                var installCache = Container().GetType<ICurrentInstalledCache>();

                var currentInstallTask = runningTasks.SingleOrDefault(t => t.PackageId == x.packageId);
                IPackage currentInstalledPackage = installCache.GetCurrentInstalledVersion(x.packageId);

                return this.ViewOrJson("package-details.cshtml", new PackageVersionsViewModel(x.packageId, packageVersions, currentInstalledPackage.Version.ToString(), currentInstallTask));
            };

            Post["/{packageId}/install", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                SemanticVersion version=null;
                string versionString = null;
                if (SemanticVersion.TryParse(Request.Form["specificVersion"], out version))
                {
                    versionString = version.ToString();
                }

                installationManager.Add(x.packageId, versionString);
                return Response.AsRedirect("/packages");
            };

            Post["/{packageId}/install/{specificVersion}", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                installationManager.Add(x.packageId, x.specificVersion);
                return Response.AsRedirect("/packages");
            };

            Post["/UpdateAllTo", y => true] = x =>
                                                  {
                string specificVersion =Response.Context.Request.Form["specificVersion"];
                var cache = Container().GetType<INuGetPackageCache>();
                var queue = Container().GetType<InstallationTaskQueue>();
                var packagesByVersion = cache.AllCachedPackages().Where(p => p.Version.Equals(new SemanticVersion(specificVersion)));

                foreach (var packageVersions in packagesByVersion)
                {
                    queue.Add(packageVersions.Id, packageVersions.Version.ToString());
                }

                return Response.AsRedirect("/packages");
            };

            Post["/UpdateAllTo/{specificVersion}", y => true] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var queue = Container().GetType<InstallationTaskQueue>();
                var packagesByVersion = cache.AllCachedPackages().Where(p=>p.Version.Equals(new SemanticVersion(x.specificVersion)));

                foreach (var packageVersions in packagesByVersion)
                {
                    queue.Add(packageVersions.Id, packageVersions.Version.ToString());
                }

                return Response.AsRedirect("/packages");
            };
        }
    }
}