using System.Collections.Generic;

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
        string[] Tags { get; }
        string LatestDirectory { get; }
        string CacheDirectory { get; }
        string BaseInstallationPath { get; }
        string MsDeployServiceUrl { get; }
        string LogsDirectory { get; }
        string HubAddress { get;  }
        int MaxConcurrentInstallations { get; }
        bool EnableConfigurationSync { get; }
        IXMPPSettings XMPPSettings { get; }
        string NotificationRecipients { get; }
    }

    public interface IXMPPSettings
    {
        bool Enabled { get; }
        string Host { get; }
        string Username { get; }
        string Password { get;  }
        int Port { get; set; }
    }
}