using System.Linq;
using Deployd.Core.PackageTransport;
using Moq;
using NUnit.Framework;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core.Test.Unit.Queries
{
    [TestFixture]
    public class RetrieveAllAvailablePackageManifestsQueryTests 
    {
        private Mock<IPackageRepository> _packageRepoMock;
        private RetrieveNuGetPackageQuery _query;
        private Mock<IPackageRepositoryFactory> _packageRepoFactoryMock;
        private Mock<ILogger> _logger = new Mock<ILogger>();

        [SetUp]
        public void SetUp()
        {
            _packageRepoMock = new Mock<IPackageRepository>();
            _packageRepoFactoryMock = new Mock<IPackageRepositoryFactory>();
            _packageRepoFactoryMock.Setup(x => x.CreateRepository(It.IsAny<string>())).Returns(_packageRepoMock.Object);
            _query = new RetrieveNuGetPackageQuery(_packageRepoFactoryMock.Object, new FeedLocation { Source = "source" }, _logger.Object);
        }

        [Test]
        public void AllAvailablePackages_CallsPackageRepoForPackageList()
        {
            var items = new IPackage[] {}.AsQueryable();
            _packageRepoMock.Setup(x => x.GetPackages()).Returns(items);

            var packages = _query.GetLatestPackage("packageId");

            _packageRepoMock.Verify(x=>x.GetPackages());
        }
    }
}
