using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class AppOfflineDeploymentHook : DeploymentHookBase
    {
        private ILog _logger = LogManager.GetLogger("WebsiteDeploymentHook");
        public AppOfflineDeploymentHook(IAgentSettings agentSettings) : base(agentSettings)
        {
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
            if (!Directory.Exists(Path.GetDirectoryName(destinationFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
            }

            if (!File.Exists(destinationFilePath))
            {
                File.Copy(tempFilePath, destinationFilePath);
            }

            // wait for the app to unload
            System.Threading.Thread.Sleep(1000);
        }


        public override void AfterDeploy(DeploymentContext context)
        {
            // delete the app_offline.htm file
            _logger.Info("Removing app_offline.htm to destination");
            string appOfflineFilePath = Path.Combine(context.TargetInstallationFolder, "app_offline.htm");
            if (File.Exists(appOfflineFilePath))
            {
                File.Delete(appOfflineFilePath);
            }
        }
    }
}
