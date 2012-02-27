using System.Collections.Generic;
using Deployd.Agent.Services.AgentConfiguration;
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
        private Mock<IAgentConfigurationManager> _agentConfigManagerMock;
        private const string PACKAGE_ID = "packageId";

        [SetUp]
        public void SetUp()
        {
            _agentConfigManagerMock = new Mock<IAgentConfigurationManager>();
            _agentConfigManagerMock.Setup(x=>x.WatchedPackages).Returns(new List<string>{PACKAGE_ID});
            _packageRepoMock = new Mock<IRetrieveAllAvailablePackageManifestsQuery>();
            _packageCacheMock = new Mock<INuGetPackageCache>();
            _pds = new PackageDownloadingService(_packageRepoMock.Object, _packageCacheMock.Object, _agentConfigManagerMock.Object);
        }

        [Test]
        public void LocallyCachePackages_CallsPackageRepoForPackageList()
        {
            var items = new List<IPackage>();
            _packageRepoMock.Setup(x => x.GetLatestPackage(PACKAGE_ID)).Returns(items);

            _pds.FetchPackages();

            _packageRepoMock.Verify(x => x.GetLatestPackage(PACKAGE_ID));
        }

    }
}
