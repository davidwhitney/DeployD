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
        }

        public IPackage GetLatestPackage(string packageId)
        {
            try
            {
                var all = _packageRepository.GetPackages().Where(x => x.Id == packageId && x.IsAbsoluteLatestVersion).ToList();

                return all.OrderByDescending(v=>v.Version).FirstOrDefault();
            } 
            catch (Exception ex)
            {
                _logger.Error("Could not get packages", ex);
                throw;
            }
        }
    }
}
