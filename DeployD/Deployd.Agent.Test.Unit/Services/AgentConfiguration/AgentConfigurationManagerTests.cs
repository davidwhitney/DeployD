using System;
using System.IO;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentConfiguration;
using NUnit.Framework;

namespace Deployd.Agent.Test.Unit.Services.AgentConfiguration
{
    [TestFixture]
    public class AgentConfigurationManagerTests
    {
        private AgentConfigurationManager _mgr;

        private const string CONFIG_FILE = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalAgentConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Environments>
    <DeploymentEnvironment>
      <Name>Staging</Name>
      <Packages>
        <string>justgiving-sdk</string>
        <string>justgivingTwo-sdk</string>
      </Packages>
    </DeploymentEnvironment>
  </Environments>
</GlobalAgentConfiguration>";

        private string _fileName;

        [SetUp]
        public void SetUp()
        {
            _fileName = Guid.NewGuid().ToString();
            File.WriteAllText(_fileName, CONFIG_FILE);
            _mgr = new AgentConfigurationManager();
        }

        [Test]
        public void ReadFromDisk_WithValidFile_HasCorrectNumberOfEnvironments()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReadFromDisk_WithValidFile_ExpectedEnvironmentNameIsCorrect()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments[0].Name, Is.EqualTo("Staging"));
        }

        [Test]
        public void ReadFromDisk_WithValidFile_HasCorrectNumberOfPackagesInKnownEnvironment()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments[0].Packages.Count, Is.EqualTo(2));
        }

        [Test]
        public void ReadFromDisk_WithValidFile_ExpectedPackageNameIsCorrect()
        {
            var configurationFile = _mgr.ReadFromDisk(_fileName);

            Assert.That(configurationFile.Environments[0].Packages[0], Is.EqualTo("justgiving-sdk"));
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
