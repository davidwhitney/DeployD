namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentSettings
    {
        int PackageSyncIntervalMs { get; }
        int ConfigurationSyncIntervalMs { get; }
        string DeploymentEnvironment { get; }
        string InstallationDirectory { get; } 
        string UnpackingLocation { get; }
        string NuGetRepository { get; }
    }
}