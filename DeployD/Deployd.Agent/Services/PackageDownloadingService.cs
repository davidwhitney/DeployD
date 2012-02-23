using System;
using Deployd.Core.Hosting;
using Deployd.Core.Queries;

namespace Deployd.Agent.Services
{
    public class PackageDownloadingService : IWindowsService
    {
        public ApplicationContext AppContext { get; set; }

        private readonly IRetrieveAllAvailablePackageManifestsQuery _allPackagesQuery;

        public PackageDownloadingService(IRetrieveAllAvailablePackageManifestsQuery allPackagesQuery)
        {
            _allPackagesQuery = allPackagesQuery;
        }

        public void Start(string[] args)
        {
            LocallyCachePackages();
        }

        public void Stop()
        {
        }

        public void LocallyCachePackages()
        {
            foreach (var package in _allPackagesQuery.AllAvailablePackages)
            {
                Console.WriteLine(package.Id);
            }
        }

    }
}
