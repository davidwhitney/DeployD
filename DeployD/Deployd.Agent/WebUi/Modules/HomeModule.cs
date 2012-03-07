using System;
using Deployd.Agent.Services.Deployment;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Nancy;
using log4net;

namespace Deployd.Agent.WebUi.Modules
{
    public class HomeModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }

        public HomeModule()
        {
            Get["/"] = x => View["index.cshtml"];
            
            Get["/packages"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var current = Container().GetType<ICurrentInstalledCache>();

                var packageListViewModel = new PackageListViewModel();
                foreach(var packageId in cache.AvailablePackages)
                {
                    var packageViewModel = new PackageViewModel {PackageId = packageId};
                    var installed = current.GetCurrentInstalledVersion(packageId);
                    if (installed != null)
                    {
                        packageViewModel.InstalledVersion = installed.Version.Version.ToString();
                    }
                    var latestAvailable = cache.GetLatestVersion(packageId);
                    if (latestAvailable != null)
                    {
                        packageViewModel.LatestAvailableVersion = latestAvailable.Version.Version.ToString();
                    }
                    packageListViewModel.Packages.Add(packageViewModel);
                }
                return View["packages.cshtml", packageListViewModel];
            };

            Get["/packages/{packageId}"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var packageVersions = cache.AvailablePackageVersions(x.packageId);
                return View["package-details.cshtml", new PackageVersionsViewModel(x.packageId, packageVersions)];
            };

            Post["/packages/{packageId}/install", y => true] = x =>
            {
                var deploymentService = Container().GetType<IDeploymentService>();
                deploymentService.InstallPackage(x.packageId);
                return Response.AsRedirect("/packages");
            };

            Post["/packages/{packageId}/install/{specificVersion}", y => true] = x =>
            {
                var deploymentService = Container().GetType<IDeploymentService>();
                deploymentService.InstallPackage(x.packageId, x.specificVersion);
                return Response.AsRedirect("/packages");
            };
        }
    }
}
