using System.Collections.Specialized;
using System.Configuration;
using System.IO.Abstractions;
using Deployd.Core.AgentConfiguration;
using Moq;
using NUnit.Framework;
using log4net;

namespace Deployd.Core.Test.Unit.AgentConfiguration
{
    [TestFixture]
    public class AgentSettingsManagerTests
    {
        private AgentSettingsManager _mgr;
        private Mock<IFileSystem> _fileSystemMock;
        private Mock<ILog> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILog>();

            _fileSystemMock = new Mock<IFileSystem>();
            _fileSystemMock.Setup(x => x.Directory.GetCurrentDirectory()).Returns("c:\\fakedirectory");
            _mgr = new AgentSettingsManager(_fileSystemMock.Object, _loggerMock.Object);            
        }

        [Test]
        public void LoadSettings_WhenSuppliedWithNoConfiguration_TakesDefaults()
        {
            var settings = _mgr.LoadSettings();

            Assert.That(settings.ConfigurationSyncIntervalMs, Is.EqualTo(60000));
            Assert.That(settings.DeploymentEnvironment, Is.EqualTo("Production"));
            Assert.That(settings.InstallationDirectory, Is.EqualTo("c:\\fakedirectory\\app_root"));
            Assert.That(settings.NuGetRepository, Is.EqualTo("c:\\fakedirectory\\DebugPackageSource"));
            Assert.That(settings.PackageSyncIntervalMs, Is.EqualTo(60000));
            Assert.That(settings.UnpackingLocation, Is.EqualTo("c:\\fakedirectory\\app_unpack"));
        }

        [Test]
        public void LoadSettings_WhenSuppliedWithConfiguration_TakesConfiguration()
        {
            KeyValueConfigurationCollection dictionary = new KeyValueConfigurationCollection
                                 {
                                     {"ConfigurationSyncIntervalMs","1"},
                                     {"DeploymentEnvironment","2"},
                                     {"InstallationDirectory","3"},
                                     {"NuGetRepository","4"},
                                     {"PackageSyncIntervalMs","5"},
                                     {"UnpackingLocation","6"},
                                 };

            var settings = _mgr.LoadSettings(dictionary);

            Assert.That(settings.ConfigurationSyncIntervalMs, Is.EqualTo(1));
            Assert.That(settings.DeploymentEnvironment, Is.EqualTo("2"));
            Assert.That(settings.InstallationDirectory, Is.StringContaining("3"));
            Assert.That(settings.NuGetRepository, Is.StringContaining("4"));
            Assert.That(settings.PackageSyncIntervalMs, Is.EqualTo(5));
            Assert.That(settings.UnpackingLocation, Is.StringContaining("6"));
        }
    }
}
