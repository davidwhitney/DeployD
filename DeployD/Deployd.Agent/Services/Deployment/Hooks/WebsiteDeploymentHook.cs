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
    public class WebsiteDeploymentHook : DeploymentHookBase
    {
        private ILog _logger = LogManager.GetLogger("WebsiteDeploymentHook");
        public WebsiteDeploymentHook(IAgentSettings agentSettings) : base(agentSettings)
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
            File.Copy(tempFilePath, destinationFilePath);

            // wait for the app to unload
            System.Threading.Thread.Sleep(1000);
        }

        public override void Deploy(DeploymentContext context)
        {
            _logger.Info("Execute msdeploy here");

            string msDeployArgsFormat =
                @"-verb:sync -source:package=""{0}"" -dest:auto,computername=""http://localhost:8090/MsDeployAgentService2/"" -skip:objectName=filePath,absolutePath=.*app_offline\.htm -skip:objectName=filePath,absolutePath=.*\.log -allowUntrusted -setParam:""IIS Web Application Name""=""{1}"" -verbose";
            string executableArgs = string.Format(msDeployArgsFormat,
                                                Path.Combine(context.WorkingFolder, "Content\\" + context.Package.Title + ".zip"),
                                                context.Package.Title);

            string executablePath = @"c:\Program Files (x86)\IIS\Microsoft Web Deploy\msdeploy.exe";
            RunProcess(executablePath, executableArgs);
        }

        private void RunProcess(string executablePath, string executableArgs)
        {
            _logger.InfoFormat("{0} {1}", executablePath, executableArgs);
            Process msDeploy = new Process();
            msDeploy.StartInfo.UseShellExecute = false;
            msDeploy.StartInfo.RedirectStandardError = true;
            msDeploy.StartInfo.RedirectStandardOutput = true;
            msDeploy.StartInfo.FileName = executablePath;
            msDeploy.StartInfo.Arguments = executableArgs;
            msDeploy.Start();

            while (!msDeploy.HasExited)
            {
                string output = msDeploy.StandardOutput.ReadToEnd();
                string error = msDeploy.StandardError.ReadToEnd();

                _logger.Info(output);
                if (error.Length > 0)
                {
                    _logger.Error(error);
                }

                msDeploy.WaitForExit(2000);
            }
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
