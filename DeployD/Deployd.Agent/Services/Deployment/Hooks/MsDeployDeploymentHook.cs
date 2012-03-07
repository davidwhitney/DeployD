using System;
using System.IO;
using System.Linq;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class MsDeployDeploymentHook : DeploymentHookBase
    {
        protected string MsWebDeployPath = string.Empty;

        private readonly string[] _knownMsWebDeployPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"IIS\Microsoft Web Deploy\msdeploy.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS\Microsoft Web Deploy\msdeploy.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"IIS\Microsoft Web Deploy V2\msdeploy.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS\Microsoft Web Deploy V2\msdeploy.exe")
        };

        public MsDeployDeploymentHook(IAgentSettings agentSettings) : base(agentSettings)
        {
            if (_knownMsWebDeployPaths.Any(File.Exists))
            {
                MsWebDeployPath = _knownMsWebDeployPaths.Last(File.Exists);
            } 
            else
            {
                if (string.IsNullOrEmpty(MsWebDeployPath))
                {
                    Logger.Fatal("Web Deploy could not be located. Ensure that Microsoft Web Deploy has been installed. Locations searched: " +
                    string.Join("\r\n", _knownMsWebDeployPaths));
                }
            }
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website")
                && !string.IsNullOrEmpty(MsWebDeployPath);
        }

        public override void Deploy(DeploymentContext context)
        {
            DeployWebsite(
                "localhost",
                Path.Combine(context.WorkingFolder, "Content\\" + context.Package.Id + ".zip"),
                context.Package.Title,
                Ignore.AppOffline().And().LogFiles().And().MaintenanceFile());
        }

        protected void DeployWebsite(string targetMachineName, string sourcePackagePath, string iisApplicationName, params string[] ignoreRegexPaths)
        {
            string ignore = string.Join(" -skip:objectName=filePath,absolutePath=", ignoreRegexPaths);
            if (ignoreRegexPaths.Length > 0)
            {
                ignore = " -skip:objectName=filePath,absolutePath=" + ignore;
            }

            const string msDeployArgsFormat = @"-verb:sync -source:package=""{0}"" -dest:auto,computername=""http://{1}:8090/MsDeployAgentService2/"" {3} -allowUntrusted -setParam:""IIS Web Application Name""=""{2}"" -verbose";
            string executableArgs = string.Format(msDeployArgsFormat,
                                                sourcePackagePath,
                                                targetMachineName,
                                                iisApplicationName,
                                                ignore);

            RunProcess(MsWebDeployPath, executableArgs);
           
        }
    }

    public class Ignore
    {
        public static string[] AppOffline()
        {
            return new[] {@".*app_offline\.htm"};
        }
        public static string[] LogFiles()
        {
            return new[] {@".*\.log"};
        }
        public static string[] MaintenanceFile()
        {
            return new[] {@".*\maintenance\.htm"};
        }
    }


    public static class IgnoreExtensions
    {
        public static string[] AppOffline(this string[] chain)
        {
            return chain.Union(new[] {@".*app_offline\.htm"}).ToArray();
        }

        public static string[] LogFiles(this string[] chain)
        {
            return chain.Union(new[] { @".*\.log" }).ToArray();
        }

        public static string[] MaintenanceFile(this string[] chain)
        {
            return chain.Union(new[] {@".*\.log"}).ToArray();
        }

        public static string[] And(this string[] chain)
        {
            return chain;
        }

    }
}
