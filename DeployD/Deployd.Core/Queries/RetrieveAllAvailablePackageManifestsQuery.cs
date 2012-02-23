using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace Deployd.Core.Queries
{
    public class RetrieveAllAvailablePackageManifestsQuery : IRetrieveAllAvailablePackageManifestsQuery
    {        
        private readonly IPackageRepository _packageRepository;

        public IList<IPackage> AllAvailablePackages 
        {
            get { return _packageRepository.GetPackages().ToList(); }
        }

        public RetrieveAllAvailablePackageManifestsQuery(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }
    }
}
