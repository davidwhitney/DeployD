using System;
using System.IO;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentConfiguration;
using Moq;
using NUnit.Framework;
using Ninject.Extensions.Logging;

namespace Deployd.Agent.Test.Unit.Services.AgentConfiguration
{
    [TestFixture]
    public class AgentConfigurationManagerTests
    {
        private AgentConfigurationManager _mgr;
        private Mock<ILogger> _logger=new Mock<ILogger>();
        private Mock<IConfigurationDefaults> _configurationDefaults = null;

        private const string CONFIG_FILE = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalAgentConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Environments>
    <DeploymentEnvironment>
      <Name>Web</Name>
      <Packages>
          <string>main-website</string>
      </Packages>
    </DeploymentEnvironment>
    <DeploymentEnvironment>
      <Name>Backoffice</Name>
      <Packages>
          <string>background-service</string>
      </Packages>
    </DeploymentEnvironment>
    <DeploymentEnvironment>
      <Name>Reporting</Name>
      <Packages>
          <string>report-update-service</string>
          <string>reporting-website</string>
      </Packages>
    </DeploymentEnvironment>
  </Environments>
</GlobalAgentConfiguration>";

        private string _fileName;

        [SetUp]
        public void SetUp()
        {
            var agentWatchList = new AgentWatchList() {Groups = new string[] {"Web", "Backoffice", "Reporting"}};
            var agentWatchListManager = new Mock<IAgentWatchListManager>();
            agentWatchListManager.Setup(m => m.Build()).Returns(agentWatchList);
            _configurationDefaults = new Mock<IConfigurationDefaults>();
            _configurationDefaults.SetupGet(c => c.AgentConfigurationFile).Returns(Guid.NewGuid().ToString());
            _configurationDefaults.SetupGet(c => c.AgentConfigurationFileLocation).Returns(Environment.CurrentDirectory);
            _fileName = Guid.NewGuid().ToString();
            File.WriteAllText(_fileName, CONFIG_FILE);
            _mgr = new AgentConfigurationManager(_logger.Object, agentWatchListManager.Object, _configurationDefaults.Object);
        }

        [Test]
        public void ReadFromDisk_WithValidFile_HasCorrectNumberOfEnvironments()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments.Count, Is.EqualTo(3));
        }

        [Test]
        public void ReadFromDisk_WithValidFile_ExpectedEnvironmentNameIsCorrect()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments[0].Name, Is.EqualTo("Web"));
        }

        [Test]
        public void ReadFromDisk_WithValidFile_HasCorrectNumberOfPackagesInKnownEnvironment()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments[0].Packages.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReadFromDisk_WithValidFile_ExpectedPackageNameIsCorrect()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments[0].Packages[0], Is.EqualTo("main-website"));
        }

        [Test]
        public void SaveToDisk_WithValidFileContents_SavesFileThatCanBeLoaded()
        {
            var testName = Guid.NewGuid().ToString();
            var testFileName = Guid.NewGuid().ToString();
            var configurationFile = new GlobalAgentConfiguration();
            configurationFile.Environments.Add(new DeploymentEnvironment());
            configurationFile.Environments[0].Name = testName;

            _mgr.SaveToDisk(configurationFile, testFileName);
            var loadedConfig = _mgr.ReadFromDisk(testFileName);

            Assert.That(loadedConfig.Environments[0].Name, Is.EqualTo(testName));
        }
    }
}
