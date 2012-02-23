using System;
using Deployd.Core.FileFormatAdapters;
using NUnit.Framework;

namespace Deployd.Core.Test.Unit.FileFormatAdapters
{
    [TestFixture]
    public class PackageReaderTests
    {
        private PackageAdapter _packageAdapter;

        [SetUp]
        public void SetUp()
        {
            _packageAdapter = new PackageAdapter();
        }

        [TestCase("")]
        [TestCase(" ")]
        public void LoadPackage_PassedEmptyWhitespace_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentException>(() => _packageAdapter.LoadPackage(path));
        }

        [TestCase(null)]
        public void LoadPackage_PassedNull_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentNullException>(() => _packageAdapter.LoadPackage(path));
        }

        [Test]
        public void LoadPackage_PassedValidPath_ReturnsParsedPackageObject()
        {
            var result = _packageAdapter.LoadPackage("test.nuspec");

            Assert.That(result, Is.Not.Null);
        }
    }
}
