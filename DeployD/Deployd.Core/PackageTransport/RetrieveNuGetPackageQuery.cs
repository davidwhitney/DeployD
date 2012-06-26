using System;
using System.Linq;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core.PackageTransport
{
    public class RetrieveNuGetPackageQuery : IRetrievePackageQuery
    {
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IPackageRepository _packageRepository;
        private readonly ILogger _logger;

        public RetrieveNuGetPackageQuery(IPackageRepositoryFactory packageRepositoryFactory, FeedLocation feedLocation, ILogger logger)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _logger = logger;
            _packageRepository = _packageRepositoryFactory.CreateRepository(feedLocation.Source);
            
            _logger.Info(string.Format("Nuget feed: {0}", feedLocation.Source));
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
