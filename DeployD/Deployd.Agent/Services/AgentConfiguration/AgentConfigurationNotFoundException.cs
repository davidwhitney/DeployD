using System;

namespace Deployd.Agent.Services.AgentConfiguration
{
    [Serializable]
    public class AgentConfigurationNotFoundException : Exception
    {
        public const string ErrorStub = "Agent configuration file was not found in package";
        public const string ErrorPattern = "Agent configuration file was not found in package '{0}'. Looking for a configuration file in the package root called '{1}'.";

        public AgentConfigurationNotFoundException()
            : base(ErrorStub)
        {
        }

        public AgentConfigurationNotFoundException(string deploydConfigurationPackageName, string configurationFileName)
            : base(string.Format(ErrorStub + " " + ErrorPattern, deploydConfigurationPackageName, configurationFileName))
        {
        }
    }
}