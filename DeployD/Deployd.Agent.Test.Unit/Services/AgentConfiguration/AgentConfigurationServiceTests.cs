using System;
using System.Threading;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentConfiguration;
using Moq;
using NUnit.Framework;

namespace Deployd.Agent.Test.Unit.Services.AgentConfiguration
{
    [TestFixture]
    public class AgentConfigurationServiceTests
    {
        private AgentConfigurationService _acs;
        private IAgentSettings _agentSettings;
        private Mock<IAgentConfigurationDownloader> _configurationDownloader;

        [SetUp]
        public void SetUp()
        {
            _agentSettings = new AgentSettings();
            _configurationDownloader = new Mock<IAgentConfigurationDownloader>();
            _acs = new AgentConfigurationService(_agentSettings, _configurationDownloader.Object);
        }

        [Test]
        public void Ctor_NullSettings_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new AgentConfigurationService(null, _configurationDownloader.Object));
            
            Assert.That(ex.ParamName, Is.EqualTo("agentSettings"));
        }

        [Test]
        public void Ctor_NullDownloader_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new AgentConfigurationService(_agentSettings, null));

            Assert.That(ex.ParamName, Is.EqualTo("configurationDownloader"));
        }

        [Test]
        public void DownloadConfiguration_WhenInvoked_CallsIAgentConfigurationDownloader()
        {
            _configurationDownloader.Setup(x => x.DownloadAgentConfiguration());

            _acs.DownloadConfiguration();

            _configurationDownloader.VerifyAll();
        }

        [Test]
        public void Start_WhenInvoked_UsesTimerToCallDownloadConfiguration()
        {
            _agentSettings.ConfigurationSyncIntervalMs = 1;
            _configurationDownloader.Setup(x => x.DownloadAgentConfiguration());

            _acs.Start(new string[0]);
            Thread.Sleep(100);

            _configurationDownloader.VerifyAll();
        }

        [Test]
        public void Stop_WhenInvoked_StopsTask()
        {
            _agentSettings.ConfigurationSyncIntervalMs = 10000;
            
            _acs.Start(new string[0]);
            Assert.That(_acs.TimedTask.IsRunning, Is.True);

            _acs.Stop();
            Assert.That(_acs.TimedTask.IsRunning, Is.False);
        }
    }
}
