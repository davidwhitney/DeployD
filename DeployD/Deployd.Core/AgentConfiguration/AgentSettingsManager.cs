using System.Configuration;
using System.IO;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettingsManager : IAgentSettingsManager
    {
        public IAgentSettings LoadSettings()
        {
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
                           DeploymentEnvironment = deploymentEnv,
                           InstallationDirectory = installationDir,
                           UnpackingLocation = unpackingLocation,
                           NuGetRepository = nuGetRepo
                       };
        }

        private string MapVirtualPath(string path)
        {
            return path.Replace("~\\", Directory.GetCurrentDirectory() + "\\");
        }
    }
}
