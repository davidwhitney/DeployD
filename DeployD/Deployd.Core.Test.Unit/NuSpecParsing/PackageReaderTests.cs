using System;
using Deployd.Core.NuSpecParsing;
using NUnit.Framework;

namespace Deployd.Agent.Test.Unit.NuSpecParsing
{
    [TestFixture]
    public class PackageReaderTests
    {
        private PackageReader _packageReader;

        [SetUp]
        public void SetUp()
        {
            _packageReader = new PackageReader();
        }

        [TestCase("")]
        [TestCase(" ")]
        public void LoadPackage_PassedEmptyWhitespace_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentException>(() => _packageReader.LoadPackage(path));
        }

        [TestCase(null)]
        public void LoadPackage_PassedNull_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => _packageReader.LoadPackage(path));
        }

        [Test]
        public void LoadPackage_PassedValidPath_ReturnsParsedPackageObject()
        {
            var result = _packageReader.LoadPackage("test.nuspec");

            Assert.That(result, Is.Not.Null);
        }
    }
}
