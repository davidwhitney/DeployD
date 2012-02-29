namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentSettings
    {
        string DeploymentEnvironment { get; set; }
        string InstallationDirectory { get; set; }
        string NuGetRepository { get; set; }
    }
}