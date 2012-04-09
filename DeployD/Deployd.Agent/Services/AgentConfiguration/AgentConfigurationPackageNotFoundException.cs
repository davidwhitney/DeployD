using System;

namespace Deployd.Agent.Services.AgentConfiguration
{
    [Serializable]
    public class AgentConfigurationPackageNotFoundException : Exception
    {
        public const string ErrorStub = "Agent configuration package was not found in package repository";

        public AgentConfigurationPackageNotFoundException()
            : base(ErrorStub)
        {
        }

        public AgentConfigurationPackageNotFoundException(string deploydConfigurationPackageName)
            : base(ErrorStub + " '" + deploydConfigurationPackageName + "'")
        {
        }
    }
}