using Deployd.Agent.Services.AgentConfiguration;
using NUnit.Framework;

namespace Deployd.Agent.Test.Unit.Services.AgentConfiguration
{
    [TestFixture]
    public class AgentConfigurationPackageNotFoundExceptionTests
    {
        [Test]
        public void Ctor_NoParams_CreatesExceptionWithDefaultHelpfulMessage()
        {
            var ex = new AgentConfigurationPackageNotFoundException();

            Assert.That(ex.Message, Is.EqualTo(AgentConfigurationPackageNotFoundException.ErrorStub));
        }

        [Test]
        public void Ctor_WithParams_CreatesExceptionWithDescriptiveHelpfulMessage()
        {
            var ex = new AgentConfigurationPackageNotFoundException("packageName");

            Assert.That(ex.Message, Is.StringContaining("packageName"));
            Assert.That(ex.Message, Is.StringContaining(AgentConfigurationPackageNotFoundException.ErrorStub));
        }
    }
}