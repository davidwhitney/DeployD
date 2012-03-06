namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettings : IAgentSettings
    {
        public int PackageSyncIntervalMs { get; set; }
        public int ConfigurationSyncIntervalMs { get; set; }
        public string DeploymentEnvironment { get; set; }
        public string InstallationDirectory { get; set; }
        public string NuGetRepository { get; set; }
        public string CacheDirectory { get; set; }

        public string[] Tags { get; set; }

        public string LatestDirectory { get; set; }

        public string UnpackingLocation { get; set; }

        public AgentSettings()
        {
            PackageSyncIntervalMs = 1000;
            ConfigurationSyncIntervalMs = 1000;
        }
    }
}
