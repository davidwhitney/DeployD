using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class AppOfflineDeploymentHook : DeploymentHookBase
    {
        private readonly IFileSystem _fileSystem;

        public AppOfflineDeploymentHook(IFileSystem fileSystem, IAgentSettings agentSettings) : base(agentSettings)
        {
            _fileSystem = fileSystem;
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website");
        }

        public override void BeforeDeploy(DeploymentContext context)
        {
            // find an app_online.htm file in the package
            var appOnline = context.Package.GetFiles().SingleOrDefault(f => f.Path.EndsWith("app_online.htm"));

            if (appOnline==null)
            {
                return;
            }

            string tempFilePath = Path.Combine(context.WorkingFolder, appOnline.Path);
            string destinationFilePath = Path.Combine(context.TargetInstallationFolder, "app_offline.htm");

            // copy the app_online file to app_offline.htm in target folder
            if (!File.Exists(tempFilePath))
            {
                return;
            }

            _logger.Info("Copying app_offline.htm to destination");
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
            {
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
            }

            if (!_fileSystem.File.Exists(destinationFilePath))
            {
                _fileSystem.File.Copy(tempFilePath, destinationFilePath);
            }

            // wait for the app to unload
            System.Threading.Thread.Sleep(1000);
        }


        public override void AfterDeploy(DeploymentContext context)
        {
            // delete the app_offline.htm file
            _logger.Info("Removing app_offline.htm to destination");
            var appOfflineFilePath = Path.Combine(context.TargetInstallationFolder, "app_offline.htm");
            
            if (_fileSystem.File.Exists(appOfflineFilePath))
            {
                _fileSystem.File.Delete(appOfflineFilePath);
            }
        }
    }
}
