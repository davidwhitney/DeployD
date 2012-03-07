using System;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class PowershellDeploymentHook : DeploymentHookBase
    {
        public PowershellDeploymentHook(IAgentSettings agentSettings) : base(agentSettings)
        {
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

        private bool ExecuteScriptIfFoundInPackage(DeploymentContext context, string scriptPath)
        {
            var file = context.Package.GetFiles().SingleOrDefault(f => f.Path.Equals(scriptPath, StringComparison.InvariantCultureIgnoreCase));
            if (file == null)
                return false;

            Logger.DebugFormat("Found script {0}, executing...", scriptPath);

            try
            {
                LoadAndExecuteScript(Path.Combine(context.WorkingFolder, file.Path));
                
            } catch (Exception ex)
            {
                Logger.Fatal("Failed executing powershell script " + file.Path, ex);
            }
            return true;
        }

        private void LoadAndExecuteScript(string pathToScript)
        {
            string serviceManagementScript = File.ReadAllText("Scripts/PS/Services.ps1");

            string scriptText = File.ReadAllText(pathToScript);


            // create Powershell runspace
            var runspace = RunspaceFactory.CreateRunspace();

            // open it
            runspace.Open();

            // create a popeline and feed it the script text
            var pipeline = runspace.CreatePipeline();

            // add our service management script
            pipeline.Commands.AddScript(serviceManagementScript);
            // add the custom script
            pipeline.Commands.AddScript(scriptText);

            // add an extra command to transform the script output objects into nicely formatted strings 
            // remove this line to get the actual objects that the script returns. For example, the script 
            // "Get-Process" returns a collection of System.Diagnostics.Process instances. 
            pipeline.Commands.Add("Out-String");

            // execute the script 
            var results = pipeline.Invoke();

            // close the runspace 
            runspace.Close();

            // convert the script result into a single string 
            var stringBuilder = new StringBuilder();
            foreach (var obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            // return the results of the script that has 
            // now been converted to text 
            Logger.Info(stringBuilder.ToString());

        }
    }
}
