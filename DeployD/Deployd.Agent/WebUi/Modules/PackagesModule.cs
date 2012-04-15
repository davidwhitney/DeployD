using System;
using System.Collections.Generic;
using System.Linq;
using Deployd.Agent.WebUi.Converters;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using Nancy;
using NuGet;

namespace Deployd.Agent.WebUi.Modules
{
    public class PackagesModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }
        public static readonly List<InstallationTask> InstallationTasks = new List<InstallationTask>();

        public PackagesModule(): base("/packages")
        {
            Get["/"] = x =>
            {
                var cache = Container().GetType<ILocalPackageCache>();
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                var installCache = Container().GetType<IInstalledPackageArchive>();
                var completedTasks = Container().GetType<CompletedInstallationTaskList>();
                var agentSettings = Container().GetType<IAgentSettings>();
                var model = RunningTasksToPackageListViewModelConverter.Convert(cache, runningTasks, installCache, completedTasks, agentSettings);
                return this.ViewOrJson("packages.cshtml", model);
            };
            
            Get["/{packageId}"] = x =>
            {
                var cache = Container().GetType<ILocalPackageCache>();
                var packageVersions = cache.AvailablePackageVersions(x.packageId);
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                var installCache = Container().GetType<IInstalledPackageArchive>();

                var currentInstallTask = runningTasks.SingleOrDefault(t => t.PackageId == x.packageId);
                IPackage currentInstalledPackage = installCache.GetCurrentInstalledVersion(x.packageId);

                return this.ViewOrJson("package-details.cshtml", new PackageVersionsViewModel(x.packageId, packageVersions, currentInstalledPackage.Version.ToString(), currentInstallTask));
            };

            Post["/{packageId}/install", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                SemanticVersion version;
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
                string specificVersion = Response.Context.Request.Form["specificVersion"];
                var cache = Container().GetType<ILocalPackageCache>();
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
                var cache = Container().GetType<ILocalPackageCache>();
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
