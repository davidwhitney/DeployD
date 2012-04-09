using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Deployd.Core.PackageCaching;
using Moq;
using NUnit.Framework;

namespace Deployd.Core.Test.Unit.Caching
{
    [TestFixture]
    public class NuGetPackageCacheTests
    {
        private Mock<IFileSystem> _fs;
        private string _cacheDir;

        [SetUp]
        public void SetUp()
        {
            _fs = new Mock<IFileSystem>();
            _cacheDir = "cache_dir";
        }

        [Test]
        public void Ctor_WithValidArgs_Constructs()
        {
            _fs.Setup(x => x.Directory.Exists(_cacheDir)).Returns(true);

            var cache = new NuGetPackageCache(_fs.Object, _cacheDir);

            Assert.That(cache, Is.Not.Null);
        }

        [Test]
        public void Ctor_WithValidArgsAndCacheDirThatDoesntExist_CreatesCacheDir()
        {
            _fs.Setup(x => x.Directory.Exists(_cacheDir)).Returns(false);

            new NuGetPackageCache(_fs.Object, _cacheDir);

            _fs.Verify(x=>x.Directory.CreateDirectory(_cacheDir));
        }

        [Test]
        public void Ctor_WithNullIFileSystem_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new NuGetPackageCache(null, _cacheDir));

            Assert.That(ex.ParamName, Is.EqualTo("fileSystem"));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Ctor_WithNullNullEmptyOrWhitespaceCacheDir_ThrowsArgumentNullException(string cacheDir)
        {
            var ex = Assert.Throws<ArgumentException>(() => new NuGetPackageCache(_fs.Object, cacheDir));

            Assert.That(ex.ParamName, Is.EqualTo("cacheDirectory"));
        }
    }
}
