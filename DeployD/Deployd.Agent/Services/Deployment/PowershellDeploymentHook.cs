using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using NuGet;
using log4net;

namespace Deployd.Agent.Services.Deployment
{
    public class PowershellDeploymentHook : IDeploymentHook
    {
        private static ILog _logger = LogManager.GetLogger("PowershellScriptRunner");
        public bool BeforeDeploy(DeploymentContext context)
        {
            return ExecuteScriptIfFoundInPackage(context, "beforedeploy.ps1");
        }

        public bool Deploy(DeploymentContext context)
        {
            return ExecuteScriptIfFoundInPackage(context, "deploy.ps1");
        }

        public bool AfterDeploy(DeploymentContext context)
        {
            return ExecuteScriptIfFoundInPackage(context, "afterdeploy.ps1");
        }

        private bool ExecuteScriptIfFoundInPackage(DeploymentContext context, string scriptPath)
        {
            var file = context.Package.GetFiles().SingleOrDefault(f => f.Path.Equals(scriptPath, StringComparison.InvariantCultureIgnoreCase));
            if (file == null)
                return false;

            _logger.DebugFormat("Found script {0}, executing...", scriptPath);

            try
            {
                LoadAndExecuteScript(Path.Combine(context.WorkingFolder, file.Path));
                
            } catch (Exception ex)
            {
                _logger.Fatal("Failed executing powershell script " + file.Path, ex);
            }
            return true;
        }

        private void LoadAndExecuteScript(string pathToScript)
        {
            string serviceManagementScript = File.ReadAllText("Scripts/PS/Services.ps1");

            string scriptText = File.ReadAllText(pathToScript);


            // create Powershell runspace
            Runspace runspace = RunspaceFactory.CreateRunspace();

            // open it
            runspace.Open();

            // create a popeline and feed it the script text
            Pipeline pipeline = runspace.CreatePipeline();

            // add our service management script
            pipeline.Commands.AddScript(serviceManagementScript);
            // add the custom script
            pipeline.Commands.AddScript(scriptText);

            // add an extra command to transform the script output objects into nicely formatted strings 
            // remove this line to get the actual objects that the script returns. For example, the script 
            // "Get-Process" returns a collection of System.Diagnostics.Process instances. 
            pipeline.Commands.Add("Out-String");

            // execute the script 
            Collection<PSObject> results = pipeline.Invoke();

            // close the runspace 
            runspace.Close();

            // convert the script result into a single string 
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            // return the results of the script that has 
            // now been converted to text 
            _logger.Info(stringBuilder.ToString());

        }
    }
}
