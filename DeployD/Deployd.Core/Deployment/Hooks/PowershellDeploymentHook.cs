using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Core.Deployment.Hooks
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
            var serviceCommands = new Command("Scripts/PS/Services.ps1");

            string scriptText = File.ReadAllText(pathToScript);


            // create Powershell runspace
            Runspace runspace = RunspaceFactory.CreateRunspace();

            // open it
            var runspace = RunspaceFactory.CreateRunspace();
            var command = new Command(pathToScript);
            command.Parameters.Add("agentEnvironment", AgentSettings.DeploymentEnvironment);

            // create Powershell runspace
            Runspace runspace = RunspaceFactory.CreateRunspace();

            // open it
            runspace.Open();
            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.Add(serviceCommands);
            
            // add the custom script
            pipeline.Commands.Add(command);

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

            logger.Info(stringBuilder.ToString());
        }
    }
}
