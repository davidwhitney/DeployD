using System.Collections.Generic;
using System.Linq;
using NuGet;
using log4net;

namespace Deployd.Core.Queries
{
    public class RetrievePackageQuery : IRetrievePackageQuery
    {
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IPackageRepository _packageRepository;
        private ILog Logger = LogManager.GetLogger("RetrievePackageQuery");

        public RetrievePackageQuery(IPackageRepositoryFactory packageRepositoryFactory, FeedLocation feedLocation)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _packageRepository = _packageRepositoryFactory.CreateRepository(feedLocation.Source);
            Logger.InfoFormat("Nuget feed: {0}", feedLocation.Source);
        }

        public IPackage GetLatestPackage(string packageId)
        {
            var all = _packageRepository.GetPackages().Where(x => x.Id == packageId && x.IsLatestVersion).ToList();
            all.Reverse();
            return all.FirstOrDefault();
        }
    }
}
