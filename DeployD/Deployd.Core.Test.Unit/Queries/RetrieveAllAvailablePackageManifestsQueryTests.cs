using System.Linq;
using Deployd.Core.Queries;
using Moq;
using NUnit.Framework;
using NuGet;

namespace Deployd.Core.Test.Unit.Queries
{
    [TestFixture]
    public class RetrieveAllAvailablePackageManifestsQueryTests 
    {
        private Mock<IPackageRepository> _packageRepoMock;
        private RetrieveAllAvailablePackageManifestsQuery _query;
        private Mock<IPackageRepositoryFactory> _packageRepoFactoryMock;

        [SetUp]
        public void SetUp()
        {
            _packageRepoMock = new Mock<IPackageRepository>();
            _packageRepoFactoryMock = new Mock<IPackageRepositoryFactory>();
            _packageRepoFactoryMock.Setup(x => x.CreateRepository(It.IsAny<string>())).Returns(_packageRepoMock.Object);
            _query = new RetrieveAllAvailablePackageManifestsQuery(_packageRepoFactoryMock.Object, new FeedLocation{ Source = "source"});
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
