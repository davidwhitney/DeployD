using System;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Core
{
    public static class PowershellHelper
    {
        public static StringBuilder ExecutePowerShellScript(string pathToScript, IAgentSettings _agentSettings)
        {
            var serviceCommands = new Command(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts/PS/Services.ps1"));

            string scriptText = File.ReadAllText(pathToScript);

            // create Powershell runspace
            Runspace runspace = RunspaceFactory.CreateRunspace();

            // open it
            var command = new Command(pathToScript);
            command.Parameters.Add("agentEnvironment", _agentSettings.DeploymentEnvironment);

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
            return stringBuilder;
        }

        public static StringBuilder ExecuteInlinePowerShellScript(string scriptText, IAgentSettings _agentSettings)
        {
            var serviceCommands = new Command(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts/PS/Services.ps1"));

            // create Powershell runspace
            Runspace runspace = RunspaceFactory.CreateRunspace();

            // open it
            runspace.Open();
            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.Add(serviceCommands);

            // add the custom script
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
            return stringBuilder;
        }
    }
}