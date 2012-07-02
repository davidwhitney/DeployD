using System;
using System.Collections.Generic;
using System.Linq;
using Deployd.Agent.WebUi.Converters;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.AgentManagement;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using Nancy;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

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
                return this.ViewOrJson("packages/index.cshtml", model);
            };
            
            Get["/{packageId}"] = x =>
            {
                var cache = Container().GetType<ILocalPackageCache>();
                var packageVersions = cache.AvailablePackageVersions(x.packageId);
                var runningTasks = Container().GetType<RunningInstallationTaskList>();
                var installCache = Container().GetType<IInstalledPackageArchive>();
                var actionsRepository = Container().GetType<IAgentActionsRepository>();

                var currentInstallTask = runningTasks.SingleOrDefault(t => t.PackageId == x.packageId);
                IPackage currentInstalledPackage = installCache.GetCurrentInstalledVersion(x.packageId);
                var availableActions = actionsRepository.GetActionsForPackage(x.packageId);

				var latestVersion = currentInstalledPackage != null ? currentInstalledPackage.Version.ToString() : "";
                return this.ViewOrJson("packages/package.cshtml",
                    new PackageVersionsViewModel(x.packageId, packageVersions, latestVersion, currentInstallTask, availableActions));
            };

            Get["/{packageId}/actions"] = x =>
            {
                var availableActionsRepository =
                    Container().GetType<IAgentActionsRepository>();
                var viewModel = new ActionListViewModel()
                {
                    Actions = availableActionsRepository.GetActionsForPackage(x.packageId),
                    PackageId = x.packageId
                };

                return this.ViewOrJson("packages/actions.cshtml", viewModel);
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
                return this.ResponseOrJson(Response.AsRedirect("/packages"));
            };

            Post["/{packageId}/install/{specificVersion}", y => true] = x =>
            {
                var installationManager = Container().GetType<InstallationTaskQueue>();
                installationManager.Add(x.packageId, x.specificVersion);
                return this.ResponseOrJson(Response.AsRedirect("/packages"));
            };

            Post["/UpdateAllTo", y => true] = x =>
            {
                var cache = Container().GetType<ILocalPackageCache>();
                var queue = Container().GetType<InstallationTaskQueue>();

                string specificVersion = Response.Context.Request.Form["specificVersion"];
                IEnumerable<IPackage> packagesToInstall = cache.AllCachedPackages().Where(p => p.Version.Equals(new SemanticVersion(specificVersion)));
                packagesToInstall = FilterPackagesByTags(packagesToInstall);

                foreach (var packageVersions in packagesToInstall)
                {
                    queue.Add(packageVersions.Id, packageVersions.Version.ToString());
                }

                return this.ResponseOrJson(Response.AsRedirect("/packages"));
            };

            Post["/UpdateAllTo/latest", y => true] = x =>
            {
                var cache = Container().GetType<ILocalPackageCache>();
                var queue = Container().GetType<InstallationTaskQueue>();


                var packagesByVersion = cache.AllCachedPackages().GroupBy(p=>p.Id, g=>g.Version);

                foreach (var packageVersions in packagesByVersion)
                {
                    queue.Add(packageVersions.Key, packageVersions.Max(g=>g.Version).ToString());
                }

                return this.ResponseOrJson(Response.AsRedirect("/packages"));
            };

            Post["/UpdateAllTo/{specificVersion}", y => true] = x =>
            {
                var cache = Container().GetType<ILocalPackageCache>();
                var queue = Container().GetType<InstallationTaskQueue>();
                IEnumerable<IPackage> packagesToInstall = 
                    cache.AllCachedPackages().Where(p=>p.Version.Equals(new SemanticVersion(x.specificVersion)));
                packagesToInstall = FilterPackagesByTags(packagesToInstall);

                foreach (var packageVersions in packagesToInstall)
                {
                    queue.Add(packageVersions.Id, packageVersions.Version.ToString());
                }

                return this.ResponseOrJson(Response.AsRedirect("/packages"));
            };
        }

        private IEnumerable<IPackage> FilterPackagesByTags(IEnumerable<IPackage> packagesToInstall)
        {
            if (Response.Context.Request.Form["all-tags"] != null
                && Response.Context.Request.Form["all-tags"] == "true"
                && Response.Context.Request.Form["tags"] != null)
            {
                throw new ArgumentException("Either select 'All' or a specific set of tags, not both");
            }

            if (Response.Context.Request.Form["tags"] != null)
            {
                string[] tags = ((string) Response.Context.Request.Form["tags"]).Split(',');
                packagesToInstall = packagesToInstall.Where(p => tags.All(t => p.Tags.Contains(t)));
            }
            return packagesToInstall;
        }
    }
}
