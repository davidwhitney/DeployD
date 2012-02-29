using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deployd.Agent.WebUi;
using NUnit.Framework;
using Nancy;
using Nancy.Testing;

namespace Deployd.Agent.Test.Unit.WebUi.Modules
{
    [TestFixture]
    public class HomeModuleTests
    {
        private Browser _browser;

        [SetUp]
        public void SetUp()
        {
            _browser = new Browser(new NancyConventionsBootstrapper());
        }

        [Test]
        public void Get_Homepage_ReturnsMarkup()
        {
            var result = _browser.Get("/", with => with.HttpRequest());

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
