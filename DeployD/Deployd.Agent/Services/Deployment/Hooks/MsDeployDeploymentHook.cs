using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class MsDeployDeploymentHook : DeploymentHookBase
    {
        private ILog _logger = LogManager.GetLogger("DefaultDeploymentHook");

        private string[] _knownMsWebDeployPaths = new[]
                                                      {
                                                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"IIS\Microsoft Web Deploy\msdeploy.exe"),
                                                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS\Microsoft Web Deploy\msdeploy.exe"),
                                                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"IIS\Microsoft Web Deploy V2\msdeploy.exe"),
                                                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS\Microsoft Web Deploy V2\msdeploy.exe"),
                                                      };

        public MsDeployDeploymentHook(IAgentSettings agentSettings) : base(agentSettings)
        {
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website");
        }

        public override void Deploy(DeploymentContext context)
        {
            if (!_knownMsWebDeployPaths.Any(p => File.Exists(p)))
            {
                _logger.Fatal("Web Deploy could not be located. Ensure that Microsoft Web Deploy has been installed. Locations searched: " +
                string.Join("\r\n", _knownMsWebDeployPaths));

                return;
            }

            _logger.Info("Execute msdeploy here");

            string msDeployArgsFormat =
                @"-verb:sync -source:package=""{0}"" -dest:auto,computername=""http://localhost:8090/MsDeployAgentService2/"" -skip:objectName=filePath,absolutePath=.*app_offline\.htm -skip:objectName=filePath,absolutePath=.*\.log -allowUntrusted -setParam:""IIS Web Application Name""=""{1}"" -verbose";
            string executableArgs = string.Format(msDeployArgsFormat,
                                                Path.Combine(context.WorkingFolder, "Content\\" + context.Package.Title + ".zip"),
                                                context.Package.Title);

            string executablePath = _knownMsWebDeployPaths
                .Last(p => File.Exists(p));

            RunProcess(executablePath, executableArgs);
        }
    }
}
