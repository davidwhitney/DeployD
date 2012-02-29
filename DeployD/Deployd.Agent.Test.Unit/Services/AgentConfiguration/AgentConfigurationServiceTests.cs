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
        public void DownloadConfiguration_WhenInvoked_CallsIAgentConfigurationDownloader()
        {
            _configurationDownloader.Setup(x => x.DownloadAgentConfiguration());

            _acs.DownloadConfiguration();

            _configurationDownloader.VerifyAll();
        }
    }
}
