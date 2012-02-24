using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace Deployd.Core.Queries
{
    public class RetrieveAllAvailablePackageManifestsQuery : IRetrieveAllAvailablePackageManifestsQuery
    {
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IPackageRepository _packageRepository;

        public IList<IPackage> AllAvailablePackages 
        {
            get
            {
                var queryable = _packageRepository.GetPackages().Where(x => x.Id == "justgiving-sdk");
                var list = queryable.ToList();
                return list;
            }
        }

        public RetrieveAllAvailablePackageManifestsQuery(FeedLocation feedLocation)
            :this(new PackageRepositoryFactory(), feedLocation)
        {
        }

        public RetrieveAllAvailablePackageManifestsQuery(IPackageRepositoryFactory packageRepositoryFactory, FeedLocation feedLocation)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _packageRepository = _packageRepositoryFactory.CreateRepository(feedLocation.Source);
        }
    }
}
