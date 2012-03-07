using System;
using Deployd.Agent.Services.Deployment;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Nancy;

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
                var cache = Container().GetType<IDeploymentService>();
                return View["packages.cshtml", new PackageListViewModel {Packages = cache.AvailablePackages()}];
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
