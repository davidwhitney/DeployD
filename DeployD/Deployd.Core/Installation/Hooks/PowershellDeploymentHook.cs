using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Core.Installation.Hooks
{
    public class PowershellDeploymentHook : DeploymentHookBase
    {
        private readonly IFileSystem _fileSystem;

        public PowershellDeploymentHook(IAgentSettings agentSettings, IFileSystem fileSystem) : base(agentSettings, fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.GetFiles().Any(f => f.Path.EndsWith(".ps1", StringComparison.CurrentCultureIgnoreCase));
        }

        public override void BeforeDeploy(DeploymentContext context)
        {
            ExecuteScriptIfFoundInPackage(context, "beforedeploy.ps1", context.GetLoggerFor(this));
        }

        public override void Deploy(DeploymentContext context)
        {
            ExecuteScriptIfFoundInPackage(context, "deploy.ps1", context.GetLoggerFor(this));
        }

        public override void AfterDeploy(DeploymentContext context)
        {
            ExecuteScriptIfFoundInPackage(context, "afterdeploy.ps1", context.GetLoggerFor(this));
        }

        private void ExecuteScriptIfFoundInPackage(DeploymentContext context, string scriptPath, ILog logger)
        {
            var file = context.Package.GetFiles().SingleOrDefault(f => f.Path.Equals(scriptPath, StringComparison.InvariantCultureIgnoreCase));
            
            if (file == null)
            {
                return;
            }

            logger.DebugFormat("Found script {0}, executing...", scriptPath);

            try
            {
                LoadAndExecuteScript(context, Path.Combine(context.WorkingFolder, file.Path), logger);
            } 
            catch (Exception ex)
            {
                logger.Fatal("Failed executing powershell script " + file.Path, ex);
            }
        }

        private void LoadAndExecuteScript(DeploymentContext context, string pathToScript, ILog logger)
        {
            var stringBuilder = PowershellHelper.ExecutePowerShellScript(pathToScript, AgentSettings);

            logger.Info(stringBuilder.ToString());
        }
    }
}
