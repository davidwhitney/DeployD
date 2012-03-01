using Deployd.Agent.Services.AgentConfiguration;
using NUnit.Framework;

namespace Deployd.Agent.Test.Unit.Services.AgentConfiguration
{
    [TestFixture]
    public class AgentConfigurationNotFoundExceptionTests
    {
        [Test]
        public void Ctor_NoParams_CreatesExceptionWithDefaultHelpfulMessage()
        {
            var ex = new AgentConfigurationNotFoundException();

            Assert.That(ex.Message, Is.EqualTo(AgentConfigurationNotFoundException.ERROR_STUB));
        }

        [Test]
        public void Ctor_WithParams_CreatesExceptionWithDescriptiveHelpfulMessage()
        {
            var ex = new AgentConfigurationNotFoundException("packageName", "fileName");

            Assert.That(ex.Message, Is.StringContaining("packageName"));
            Assert.That(ex.Message, Is.StringContaining("fileName"));
            Assert.That(ex.Message, Is.StringContaining(AgentConfigurationNotFoundException.ERROR_STUB));
        }
    }
}
