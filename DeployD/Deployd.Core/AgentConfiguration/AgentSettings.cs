using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettings : IAgentSettings
    {
        public string DeploymentEnvironment { get; set; }
        public string InstallationDirectory { get; set; }
        public string NuGetRepository { get; set; }
        public string UnpackingLocation { get; set; }
    }
}
