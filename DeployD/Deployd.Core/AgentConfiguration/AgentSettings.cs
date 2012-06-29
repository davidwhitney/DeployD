using System;
using System.IO;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettings : IAgentSettings
    {
        //public static readonly string AgentProgramDataPath = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string AgentProgramDataPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DeployD.Agent");

        public int PackageSyncIntervalMs { get; set; }
        public int ConfigurationSyncIntervalMs { get; set; }
        public string DeploymentEnvironment { get; set; }
        public string InstallationDirectory { get; set; }
        public string NuGetRepository { get; set; }
        public string CacheDirectory { get; set; }

        public string[] Tags { get; set; }

        public string LatestDirectory { get; set; }

        public string UnpackingLocation { get; set; }

        public string BaseInstallationPath { get; set; }

        public string MsDeployServiceUrl { get; set; }

        public string LogsDirectory { get; set; }

        public string HubAddress { get; set; }

        public AgentSettings()
        {
            PackageSyncIntervalMs = 1000;
            ConfigurationSyncIntervalMs = 1000;
        }
    }
}
