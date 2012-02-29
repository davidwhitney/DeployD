using System;

namespace Deployd.Agent.Services.AgentConfiguration
{
    [Serializable]
    public class AgentConfigurationNotFoundException : Exception
    {
        public const string ERROR_STUB = "Agent configuration file was not found in package";
        public const string ERROR_PATTERN = "Agent configuration file was not found in package '{0}'. Looking for a configuration file in the package root called '{1}'.";

        public AgentConfigurationNotFoundException()
            : base(ERROR_STUB)
        {
        }

        public AgentConfigurationNotFoundException(string deploydConfigurationPackageName, string configurationFileName)
            : base(string.Format(ERROR_STUB + " " + ERROR_PATTERN, deploydConfigurationPackageName, configurationFileName))
        {
        }
    }
}