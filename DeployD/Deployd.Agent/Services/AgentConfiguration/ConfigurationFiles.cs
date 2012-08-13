using System;
using System.IO;
using Deployd.Core;

namespace Deployd.Agent.Services.AgentConfiguration
{
    public class ConfigurationDefaults : IConfigurationDefaults
    {
        public string AgentConfigurationFile { get { return @"GlobalAgentConfiguration.xml"; } }
        public string AgentConfigurationFileLocation { get { return @"~\"; } }
    }
}
