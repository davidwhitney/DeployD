﻿using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace Deployd.Core.Queries
{
    public class RetrievePackageQuery : IRetrievePackageQuery
    {
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IPackageRepository _packageRepository;

        public RetrievePackageQuery(IPackageRepositoryFactory packageRepositoryFactory, FeedLocation feedLocation)
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
