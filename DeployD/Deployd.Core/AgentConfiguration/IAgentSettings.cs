namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentSettings
    {
        int PackageSyncIntervalMs { get; set; }
        int ConfigurationSyncIntervalMs { get; set; }
        string DeploymentEnvironment { get; set; }
        string InstallationDirectory { get; set; } 
        string UnpackingLocation { get; set; }
        string NuGetRepository { get; set; }
    }
}