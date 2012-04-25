using System;
using System.Linq;
using NuGet;
using log4net;

namespace Deployd.Core.PackageTransport
{
    public class RetrieveNuGetPackageQuery : IRetrievePackageQuery
    {
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IPackageRepository _packageRepository;
        private readonly ILog _logger = LogManager.GetLogger("RetrieveNuGetPackageQuery");

        public RetrieveNuGetPackageQuery(IPackageRepositoryFactory packageRepositoryFactory, FeedLocation feedLocation)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _packageRepository = _packageRepositoryFactory.CreateRepository(feedLocation.Source);
            
            _logger.InfoFormat("Nuget feed: {0}", feedLocation.Source);
        }

        public IPackage GetLatestPackage(string packageId)
        {
            try
            {
                var all = _packageRepository.GetPackages().Where(x => x.Id == packageId && x.IsLatestVersion).ToList();
                all.Reverse();
                return all.FirstOrDefault();
            } 
            catch (Exception ex)
            {
                _logger.Error("Could not get packages", ex);
                throw;
            }
        }
    }
}
