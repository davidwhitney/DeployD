using System;

namespace Deployd.Agent.Services.AgentConfiguration
{
    [Serializable]
    public class AgentConfigurationPackageNotFoundException : Exception
    {
        public const string ERROR_STUB = "Agent configuration package was not found in package repository";

        public AgentConfigurationPackageNotFoundException()
            : base(ERROR_STUB)
        {
        }

        public AgentConfigurationPackageNotFoundException(string deploydConfigurationPackageName)
            : base(ERROR_STUB + " '" + deploydConfigurationPackageName + "'")
        {
        }
    }
}