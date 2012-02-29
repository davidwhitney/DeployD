using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deployd.Agent.Services.Deployment;
using Deployd.Agent.WebUi;
using Deployd.Agent.WebUi.Modules;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Moq;
using NUnit.Framework;
using Nancy;
using Nancy.Testing;
using NuGet;

namespace Deployd.Agent.Test.Unit.WebUi.Modules
{
    [TestFixture]
    public class HomeModuleTests
    {
        private Browser _browser;
        private ContainerStub _containerStub;

        [SetUp]
        public void SetUp()
        {
            _containerStub = new ContainerStub
                                 {
                                     NuGetPackageCacheMock = new Mock<INuGetPackageCache>(),
                                     DeploymentServiceMock = new Mock<IDeploymentService>()
                                 };

            HomeModule.Container = () => _containerStub;
            _browser = new Browser(new NancyConventionsBootstrapper());
        }

        [Test]
        public void Get_Homepage_ReturnsMarkup()
        {
            var result = _browser.Get("/", with => with.HttpRequest());

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void Get_Packages_ReturnsMarkupWithPackagesFromCache()
        {
            _containerStub.NuGetPackageCacheMock.Setup(x => x.AvailablePackages).Returns(new List<string>{"package1", "package2"});
            
            var result = _browser.Get("/packages", with => with.HttpRequest());

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Body.AsString().Contains("package1"));
            Assert.That(result.Body.AsString().Contains("package2"));
        }

        [Test]
        public void Get_PackageById_ReturnsMarkupWithAvailableVersionsFromCache()
        {
            _containerStub.NuGetPackageCacheMock.Setup(x => x.AvailablePackageVersions("mypackage")).Returns(new List<string> { "ver1", "ver2" });
            
            var result = _browser.Get("/packages/mypackage", with => with.HttpRequest());

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Body.AsString().Contains("ver1"));
            Assert.That(result.Body.AsString().Contains("ver2"));
        }

        [Test]
        public void Get_PackageByIdInstall_InvokesInstallOnInstallationService()
        {
            var mockPackage = new Mock<IPackage>();
            _containerStub.NuGetPackageCacheMock.Setup(x => x.GetLatestVersion("mypackage")).Returns(mockPackage.Object);

            var result = _browser.Post("/packages/mypackage/install", with => with.HttpRequest());

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            _containerStub.DeploymentServiceMock.Verify(x=>x.Deploy(mockPackage.Object));

        }

        public class ContainerStub : IIocContainer
        {
            public Mock<INuGetPackageCache> NuGetPackageCacheMock { get; set; }
            public Mock<IDeploymentService> DeploymentServiceMock { get; set; }

            public T GetType<T>()
            {
                if (typeof(T) == typeof(INuGetPackageCache))
                {
                    return (T)NuGetPackageCacheMock.Object;
                }

                if (typeof(T) == typeof(IDeploymentService))
                {
                    return (T)DeploymentServiceMock.Object;
                }

                throw new NotImplementedException();
            }
        }
    }
}
