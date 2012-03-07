using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class PowershellDeploymentHook : DeploymentHookBase
    {
        private readonly IFileSystem _fileSystem;

        public PowershellDeploymentHook(IAgentSettings agentSettings, IFileSystem fileSystem) : base(agentSettings)
        {
            _fileSystem = fileSystem;
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.GetFiles().Any(f => f.Path.EndsWith(".ps1", StringComparison.CurrentCultureIgnoreCase));
        }

        public override void BeforeDeploy(DeploymentContext context)
        {
            ExecuteScriptIfFoundInPackage(context, "beforedeploy.ps1");
        }

        public override void Deploy(DeploymentContext context)
        {
            ExecuteScriptIfFoundInPackage(context, "deploy.ps1");
        }

        public override void AfterDeploy(DeploymentContext context)
        {
            ExecuteScriptIfFoundInPackage(context, "afterdeploy.ps1");
        }

        private void ExecuteScriptIfFoundInPackage(DeploymentContext context, string scriptPath)
        {
            var file = context.Package.GetFiles().SingleOrDefault(f => f.Path.Equals(scriptPath, StringComparison.InvariantCultureIgnoreCase));
            
            if (file == null)
            {
                return;
            }

            Logger.DebugFormat("Found script {0}, executing...", scriptPath);

            try
            {
                LoadAndExecuteScript(Path.Combine(context.WorkingFolder, file.Path));
            } 
            catch (Exception ex)
            {
                Logger.Fatal("Failed executing powershell script " + file.Path, ex);
            }
        }

        private void LoadAndExecuteScript(string pathToScript)
        {
            var serviceManagementScript = File.ReadAllText("Scripts/PS/Services.ps1");
            var scriptText = _fileSystem.File.ReadAllText(pathToScript);

            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(serviceManagementScript);
            pipeline.Commands.AddScript(scriptText);

            // add an extra command to transform the script output objects into nicely formatted strings 
            // remove this line to get the actual objects that the script returns. For example, the script 
            // "Get-Process" returns a collection of System.Diagnostics.Process instances. 
            pipeline.Commands.Add("Out-String");

            var results = pipeline.Invoke();
            runspace.Close();

            // convert the script result into a single string 
            var stringBuilder = new StringBuilder();
            foreach (var obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            Logger.Info(stringBuilder.ToString());
        }
    }
}
