using System;
using System.Collections.Generic;
using System.Linq;
using Deployd.Core.Hosting;
using NuGet;

namespace Deployd.Agent
{
    public class PackageDownloadingService : IWindowsService
    {
        public ApplicationContext AppContext { get; set; }

        private readonly IPackageRepository _packageRepository;

        public IList<IPackage> AllAvailablePackages 
        {
            get { return _packageRepository.GetPackages().ToList(); }
        }

        public PackageDownloadingService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public void Start(string[] args)
        {
        }

        public void Stop()
        {
        }

        public void LocallyCachePackages()
        {
            foreach (var package in AllAvailablePackages)
            {
                Console.WriteLine(package.Id);
            }
        }

    }
}
