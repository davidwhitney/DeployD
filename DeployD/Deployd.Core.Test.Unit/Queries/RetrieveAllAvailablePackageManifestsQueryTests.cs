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

        [SetUp]
        public void SetUp()
        {
            _packageRepoMock = new Mock<IPackageRepository>();
            _query = new RetrieveAllAvailablePackageManifestsQuery(_packageRepoMock.Object);
        }

        [Test]
        public void AllAvailablePackages_CallsPackageRepoForPackageList()
        {
            var items = new IPackage[] {}.AsQueryable();
            _packageRepoMock.Setup(x => x.GetPackages()).Returns(items);

            var packages = _query.AllAvailablePackages;

            _packageRepoMock.Verify(x=>x.GetPackages());
        }
    }
}
