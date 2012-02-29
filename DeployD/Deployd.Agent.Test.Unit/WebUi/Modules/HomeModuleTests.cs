using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deployd.Agent.WebUi;
using Deployd.Agent.WebUi.Modules;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Moq;
using NUnit.Framework;
using Nancy;
using Nancy.Testing;

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
                                     NuGetPackageCacheMock = new Mock<INuGetPackageCache>()
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

        public class ContainerStub : IIocContainer
        {
            public Mock<INuGetPackageCache> NuGetPackageCacheMock { get; set; }

            public T GetType<T>()
            {
                if (typeof(T) == typeof(INuGetPackageCache))
                {
                    return (T)NuGetPackageCacheMock.Object;
                }

                throw new NotImplementedException();
            }
        }
    }
}
