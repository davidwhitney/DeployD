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
            var nuGetRepo = ConfigurationManager.AppSettings["NuGetRepository"] ?? "~\\DebugPackageSource";

            installationDir = MapVirtualPath(installationDir);
            nuGetRepo = MapVirtualPath(nuGetRepo);

            DirectoryHelpers.EnsureExists(installationDir);

            return new AgentSettings
                       {
                           DeploymentEnvironment = deploymentEnv,
                           InstallationDirectory = installationDir,
                           NuGetRepository = nuGetRepo
                       };
        }

        private string MapVirtualPath(string path)
        {
            return path.Replace("~\\", Directory.GetCurrentDirectory() + "\\");
        }
    }
}
