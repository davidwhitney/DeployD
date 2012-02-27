﻿using System;
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
                var cache = Container().GetType<INuGetPackageCache>();
                var packageList = cache.AvailablePackages;
                return View["packages.cshtml", packageList];
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
                return HttpStatusCode.OK;
            };

            Post["/packages/{packageId}/install/{specificVersion}", y => true] = x =>
            {
                // install specific
                string specificVersion = x.specificVersion;

                return HttpStatusCode.OK;
            };
        }
    }
}