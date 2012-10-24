﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.PackageTransport;
using Moq;
using NUnit.Framework;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.Test.Unit.Services.AgentConfiguration
{
    [TestFixture]
    public class AgentConfigurationDownloaderTests
    {
        private AgentConfigurationDownloader _downloader;
        private Mock<IAgentConfigurationManager> _agentConfigManagerMock;
        private Mock<IRetrievePackageQuery> _retrieveQueryMock;
        private Mock<IPackage> _nugetPackageMock;
        private PackageFileStub _nugetPackageFile;
        private Mock<IAgentSettingsManager> _agentSettingsManagerMock;
        private Mock<ILogger> _logger=new Mock<ILogger>();
        private Mock<IConfigurationDefaults> _configurationDefaults = null;

        [SetUp]
        public void SetUp()
        {
            _configurationDefaults = new Mock<IConfigurationDefaults>();
            _configurationDefaults.SetupGet(c => c.AgentConfigurationFile).Returns(Guid.NewGuid().ToString());
            _configurationDefaults.SetupGet(c => c.AgentConfigurationFileLocation).Returns(Environment.CurrentDirectory);
            _agentSettingsManagerMock = new Mock<IAgentSettingsManager>();
            _nugetPackageMock = new Mock<IPackage>();
            _nugetPackageFile = new PackageFileStub { Path = Environment.CurrentDirectory };
            _agentConfigManagerMock = new Mock<IAgentConfigurationManager>();
            _retrieveQueryMock = new Mock<IRetrievePackageQuery>();
            _downloader = new AgentConfigurationDownloader(_agentConfigManagerMock.Object, _retrieveQueryMock.Object, _agentSettingsManagerMock.Object, _logger.Object, _configurationDefaults.Object);
        }

        [Test]
        public void DownloadAgentConfiguration_WhenConfigPackageDoesntExist_ThrowsAgentConfigurationPackageNotFoundException()
        {
            IPackage packageFile = null;
            _retrieveQueryMock.Setup(x => x.GetLatestPackage(AgentConfigurationDownloader.DeploydConfigurationPackageName)).Returns(packageFile);

            Assert.Throws<AgentConfigurationPackageNotFoundException>(() => _downloader.DownloadAgentConfiguration());
        }

        [Test]
        public void DownloadAgentConfiguration_WhenConfigPackageDoesntHaveAgentConfiguration_ThrowsAgentConfigurationNotFoundException()
        {
            var packageFile =_nugetPackageMock.Object;
            _nugetPackageMock.Setup(x => x.GetFiles()).Returns(new List<IPackageFile>());
            _retrieveQueryMock.Setup(x => x.GetLatestPackage(AgentConfigurationDownloader.DeploydConfigurationPackageName)).Returns(packageFile);

            Assert.Throws<AgentConfigurationNotFoundException>(() => _downloader.DownloadAgentConfiguration());
        }

        [Test]
        public void DownloadAgentConfiguration_WithTargetFile_DownloadsPackageWithDefaultNameUsingQuery()
        {
            SetupMockPackageDownload();

            _downloader.DownloadAgentConfiguration();

            _retrieveQueryMock.VerifyAll();
        }

        [Test]
        public void DownloadAgentConfiguration_WithTargetFile_SavesDownloadedPackageToDisk()
        {
            SetupMockPackageDownload();
            _nugetPackageFile.UnderlyingStream = new MemoryStream(new byte[] {66});

            _downloader.DownloadAgentConfiguration();

            _agentConfigManagerMock.Verify(x=>x.SaveToDisk(It.Is<byte[]>(y=>y[0] == 66), It.IsAny<string>()));
        }

        private void SetupMockPackageDownload()
        {
            var packageContents = new List<IPackageFile> {_nugetPackageFile};
            var packageFile = _nugetPackageMock.Object;
            _nugetPackageMock.Setup(x => x.GetFiles()).Returns(packageContents);
            _retrieveQueryMock.Setup(x => x.GetLatestPackage(AgentConfigurationDownloader.DeploydConfigurationPackageName)).Returns(packageFile);
        }
    }

    public class PackageFileStub : IPackageFile
    {
        public Stream UnderlyingStream { get; set; }

        public PackageFileStub()
        {
            UnderlyingStream = new MemoryStream();
        }

        public Stream GetStream()
        {
            return UnderlyingStream;
        }

        public string Path { get; set; }

        public string EffectivePath
        {
            get { throw new System.NotImplementedException(); }
        }

        public FrameworkName TargetFramework
        {
            get { throw new System.NotImplementedException(); }
        }

        public IEnumerable<FrameworkName> SupportedFrameworks
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
