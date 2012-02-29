using System.IO.Abstractions;
using Moq;
using NUnit.Framework;

namespace Deployd.Core.Test.Unit
{
    [TestFixture]
    public class FileSystemExtensionsTests
    {
        private Mock<IFileSystem> _fs;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _fs = new Mock<IFileSystem>();
            _path = "path";
        }

        [Test]
        public void EnsureDirectoryExists_DirectoryExists_Returns()
        {
            _fs.Setup(x => x.Directory.Exists(_path)).Returns(true);

            _fs.Object.EnsureDirectoryExists(_path);

            _fs.Verify(x=>x.Directory.CreateDirectory(_path), Times.Never());
        }

        [Test]
        public void EnsureDirectoryExists_DirectoryDoesNotExist_CreatesDirectory()
        {
            _fs.Setup(x => x.Directory.Exists(_path)).Returns(false);

            _fs.Object.EnsureDirectoryExists(_path);

            _fs.Verify(x=>x.Directory.CreateDirectory(_path), Times.Once());
        }
    }
}
