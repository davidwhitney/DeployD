using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace Deployd.Core.Queries
{
    public class RetrieveAllAvailablePackageManifestsQuery : IRetrieveAllAvailablePackageManifestsQuery
    {
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IPackageRepository _packageRepository;
        
        public RetrieveAllAvailablePackageManifestsQuery(FeedLocation feedLocation)
            :this(new PackageRepositoryFactory(), feedLocation)
        {
        }

        public RetrieveAllAvailablePackageManifestsQuery(IPackageRepositoryFactory packageRepositoryFactory, FeedLocation feedLocation)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _packageRepository = _packageRepositoryFactory.CreateRepository(feedLocation.Source);
        }

        public IList<IPackage> GetLatestPackage(string packageId)
        {
            return _packageRepository.GetPackages().Where(x => x.Id == packageId && x.IsLatestVersion).ToList();
        }
    }
}
