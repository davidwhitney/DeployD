using System.Collections.Generic;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core.Caching;
using Deployd.Core.Queries;
using Moq;
using NUnit.Framework;
using NuGet;

namespace Deployd.Agent.Test.Unit.Services.PackageDownloading
{
    [TestFixture]
    public class PackageDownloadingServiceTests 
    {
        private Mock<IRetrieveAllAvailablePackageManifestsQuery> _packageRepoMock;
        private Mock<INuGetPackageCache> _packageCacheMock;
        private PackageDownloadingService _pds;

        [SetUp]
        public void SetUp()
        {
            _packageRepoMock = new Mock<IRetrieveAllAvailablePackageManifestsQuery>();
            _packageCacheMock = new Mock<INuGetPackageCache>();
            _pds = new PackageDownloadingService(_packageRepoMock.Object, _packageCacheMock.Object);
        }

        [Test]
        public void LocallyCachePackages_CallsPackageRepoForPackageList()
        {
            var items = new List<IPackage>();
            _packageRepoMock.Setup(x => x.AllAvailablePackages).Returns(items);

            _pds.LocallyCachePackages();

            _packageRepoMock.Verify(x => x.AllAvailablePackages);
        }

    }
}
