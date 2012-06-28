using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Core.Installation.Hooks
{
    public class AppOfflineDeploymentHook : DeploymentHookBase
    {
        private readonly IFileSystem _fileSystem;
        private const string APP_ONLINE_FILE = "app_online.htm";
        private const string APP_OFFLINE_FILE = "app_offline.htm";

        public AppOfflineDeploymentHook(IFileSystem fileSystem, IAgentSettingsManager agentSettingsManager) : base(agentSettingsManager, fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website");
        }

        public override void BeforeDeploy(DeploymentContext context)
        {
            var logger = context.GetLoggerFor(this);
            // find an app_online.htm file in the package
            var appOnline = context.Package.GetFiles().SingleOrDefault(f => f.Path.EndsWith(APP_ONLINE_FILE));

            if (appOnline == null)
            {
                return;
            }

            var tempFilePath = Path.Combine(context.WorkingFolder, appOnline.Path);
            var destinationFilePath = Path.Combine(context.TargetInstallationFolder, APP_OFFLINE_FILE);

            if (!File.Exists(tempFilePath))
            {
                return;
            }

            logger.Info("Copying app_offline.htm to destination");
            
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
            {
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
            }

            if (!_fileSystem.File.Exists(destinationFilePath))
            {
                _fileSystem.File.Copy(tempFilePath, destinationFilePath);
            }

            WaitForAppToUnload();
        }

        private static void WaitForAppToUnload()
        {
            System.Threading.Thread.Sleep(1000);
        }

        public override void AfterDeploy(DeploymentContext context)
        {
            var logger = context.GetLoggerFor(this);
            logger.Info("Removing app_offline.htm to destination");

            var appOfflineFilePath = Path.Combine(context.TargetInstallationFolder, APP_OFFLINE_FILE);
            
            if (_fileSystem.File.Exists(appOfflineFilePath))
            {
                _fileSystem.File.Delete(appOfflineFilePath);
            }
        }
    }
}
