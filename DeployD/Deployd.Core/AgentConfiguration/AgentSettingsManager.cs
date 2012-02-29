using System;
using System.Configuration;
using System.IO;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettingsManager : IAgentSettingsManager
    {
        public IAgentSettings LoadSettings()
        {
            var packageSyncIntervalMs = ConfigurationManager.AppSettings["PackageSyncIntervalMs"] ?? "60000";
            var configurationSyncIntervalMs = ConfigurationManager.AppSettings["ConfigurationSyncIntervalMs"] ?? "60000";
            var deploymentEnv = ConfigurationManager.AppSettings["DeploymentEnvironment"] ?? "Production";
            var installationDir = ConfigurationManager.AppSettings["InstallationDirectory"] ?? "~\\app_root";
            var unpackingLocation = ConfigurationManager.AppSettings["UnpackingLocation"] ?? "~\\app_unpack";
            var nuGetRepo = ConfigurationManager.AppSettings["NuGetRepository"] ?? "~\\DebugPackageSource";

            installationDir = MapVirtualPath(installationDir);
            unpackingLocation = MapVirtualPath(unpackingLocation);
            nuGetRepo = MapVirtualPath(nuGetRepo);

            DirectoryHelpers.EnsureExists(installationDir);
            DirectoryHelpers.EnsureExists(unpackingLocation);

            return new AgentSettings
                       {
                           PackageSyncIntervalMs = Int32.Parse(packageSyncIntervalMs),
                           ConfigurationSyncIntervalMs = Int32.Parse(configurationSyncIntervalMs),
                           DeploymentEnvironment = deploymentEnv,
                           InstallationDirectory = installationDir,
                           UnpackingLocation = unpackingLocation,
                           NuGetRepository = nuGetRepo
                       };
        }

        private static string MapVirtualPath(string path)
        {
            return path.Replace("~\\", Directory.GetCurrentDirectory() + "\\");
        }
    }
}
