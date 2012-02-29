using Deployd.Core.AgentConfiguration;
using NUnit.Framework;

namespace Deployd.Core.Test.Unit.AgentConfiguration
{
    [TestFixture]
    public class AgentSettingsManagerTests
    {
        [Test]
        public void Placeholder()
        {
            var mgr = new AgentSettingsManager();
            var settings = mgr.LoadSettings();
        }
    }
}
