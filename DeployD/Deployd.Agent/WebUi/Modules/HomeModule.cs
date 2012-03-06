using System;
using System.Collections.Generic;
using System.Linq;
using Deployd.Agent.Services.Deployment;
using Deployd.Agent.WebUi.Models;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Nancy;
using Nancy.Responses;
using log4net;

namespace Deployd.Agent.WebUi.Modules
{
    public class HomeModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }
        private ILog _logger = LogManager.GetLogger("HomeModule");

        public HomeModule()
        {
            Get["/"] = x => View["index.cshtml"];
            
            Get["/packages"] = x =>
            {
                var cache = Container().GetType<INuGetPackageCache>();
                var current = Container().GetType<ICurrentInstalledCache>();
                var packageList = cache.AvailablePackages;

                var packageListViewModel = new PackageListViewModel();
                packageListViewModel.Packages = new List<PackageViewModel>();
                foreach(var packageId in packageList)
                {
                    var packageViewModel = new PackageViewModel();
                    packageViewModel.PackageId = packageId;
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
                // install latest
                var cache = Container().GetType<INuGetPackageCache>();
                var package = cache.GetLatestVersion(x.packageId);
                

                // deploy
                var deploymentService = Container().GetType<IDeploymentService>();
                deploymentService.Deploy(package);

                // write installation marker
                var installationCache = Container().GetType<ICurrentInstalledCache>();
                try
                {
                    installationCache.SetCurrentInstalledVersion(package);
                } catch( Exception ex)
                {
                    _logger.Warn("Could not set current installed version", ex);
                }

                return Response.AsRedirect("/packages");
            };

            Post["/packages/{packageId}/install/{specificVersion}", y => true] = x =>
            {
                // find specific version if available
                var cache = Container().GetType<INuGetPackageCache>();
                var package = cache.GetSpecificVersion(x.packageId, x.specificVersion);

                // deploy
                var deploymentService = Container().GetType<IDeploymentService>();
                deploymentService.Deploy(package);

                // write installation marker
                var installationCache = Container().GetType<ICurrentInstalledCache>();
                try
                {
                    installationCache.SetCurrentInstalledVersion(package);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Could not set current installed version", ex);
                }

                return Response.AsRedirect("/packages");
            };
        }
    }
}
