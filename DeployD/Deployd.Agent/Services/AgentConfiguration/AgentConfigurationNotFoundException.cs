using System;
using System.Runtime.Serialization;

namespace Deployd.Agent.Services.AgentConfiguration
{
    [Serializable]
    public class AgentConfigurationNotFoundException : Exception
    {
        public AgentConfigurationNotFoundException()
        {
        }

        public AgentConfigurationNotFoundException(string message) : base(message)
        {
        }

        public AgentConfigurationNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AgentConfigurationNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}