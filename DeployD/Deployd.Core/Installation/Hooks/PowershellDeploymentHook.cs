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

        public PowershellDeploymentHook(IAgentSettingsManager agentSettingsManager, IFileSystem fileSystem) : base(agentSettingsManager, fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.GetFiles().Any(f => f.Path.EndsWith(".ps1", StringComparison.CurrentCultureIgnoreCase));
        }

        public override void BeforeDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ExecuteScriptIfFoundInPackage(context, "beforedeploy.ps1", context.GetLoggerFor(this), reportProgress);
        }

        public override void Deploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ExecuteScriptIfFoundInPackage(context, "deploy.ps1", context.GetLoggerFor(this), reportProgress);
        }

        public override void AfterDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            ExecuteScriptIfFoundInPackage(context, "afterdeploy.ps1", context.GetLoggerFor(this), reportProgress);
        }

        public override string ProgressMessage
        {
            get { return "Running PowerShell scripts"; }
        }

        private void ExecuteScriptIfFoundInPackage(DeploymentContext context, string scriptPath, ILog logger, Action<ProgressReport> reportProgress)
        {
            var file = context.Package.GetFiles().SingleOrDefault(f => f.Path.Equals(scriptPath, StringComparison.InvariantCultureIgnoreCase));
            
            if (file == null)
            {
                return;
            }

            reportProgress(new ProgressReport(context, GetType(), "Running " + Path.GetFileName(file.Path)));
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
            var stringBuilder = PowershellHelper.ExecutePowerShellScript(pathToScript, AgentSettingsManager.Settings);

            logger.Info(stringBuilder.ToString());
        }
    }
}
