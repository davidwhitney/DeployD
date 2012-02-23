using System.Linq;
using Moq;
using NUnit.Framework;
using NuGet;

namespace Deployd.Agent.Test.Unit
{
    [TestFixture]
    public class PackageDownloadingServiceTests 
    {
        private Mock<IPackageRepository> _packageRepoMock;
        private PackageDownloadingService _pds;

        [SetUp]
        public void SetUp()
        {
            _packageRepoMock = new Mock<IPackageRepository>();
            _pds = new PackageDownloadingService(_packageRepoMock.Object);
        }

        [Test]
        public void AllAvailablePackages_CallsPackageRepoForPackageList()
        {
            var items = new IPackage[] {}.AsQueryable();
            _packageRepoMock.Setup(x => x.GetPackages()).Returns(items);

            var packages = _pds.AllAvailablePackages;

            _packageRepoMock.Verify(x=>x.GetPackages());
        }

    }
}
