using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettings : IAgentSettings
    {
        public string DeploymentEnvironment { get; set; }
    }
}
