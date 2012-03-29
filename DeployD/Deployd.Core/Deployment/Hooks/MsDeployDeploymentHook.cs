using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using log4net;
using log4net.Repository.Hierarchy;

namespace Deployd.Core.Deployment.Hooks
{
    public class MsDeployDeploymentHook : DeploymentHookBase
    {
        private readonly IFileSystem _fileSystem;
        protected string MsWebDeployPath = string.Empty;

        private readonly string[] _knownMsWebDeployPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"IIS\Microsoft Web Deploy\msdeploy.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS\Microsoft Web Deploy\msdeploy.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"IIS\Microsoft Web Deploy V2\msdeploy.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS\Microsoft Web Deploy V2\msdeploy.exe")
        };

        public MsDeployDeploymentHook(IAgentSettings agentSettings, IFileSystem fileSystem) : base(agentSettings, fileSystem)
        {
            _fileSystem = fileSystem;
        }

        private void LocateMsDeploy(ILog logger)
        {
            if (_knownMsWebDeployPaths.Any(_fileSystem.File.Exists))
            {
                MsWebDeployPath = _knownMsWebDeployPaths.Last(_fileSystem.File.Exists);
            }
            else
            {
                if (string.IsNullOrEmpty(MsWebDeployPath))
                {
                    logger.Fatal(
                        "Web Deploy could not be located. Ensure that Microsoft Web Deploy has been installed. Locations searched: " +
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
            var installationLogger = context.GetLoggerFor(this);
            LocateMsDeploy(installationLogger);
            DeployWebsite(
                "localhost",
                Path.Combine(context.WorkingFolder, "Content\\" + context.Package.Id + ".zip"),
                context.Package.Title,
                installationLogger,
                Ignore.AppOffline().And().LogFiles().And().MaintenanceFile());
        }

        protected void DeployWebsite(string targetMachineName, string sourcePackagePath, string iisApplicationName, ILog logger, params string[] ignoreRegexPaths)
        {
            var ignore = string.Join(" -skip:objectName=filePath,absolutePath=", ignoreRegexPaths);
            
            if (ignoreRegexPaths.Length > 0)
            {
                ignore = " -skip:objectName=filePath,absolutePath=" + ignore;
            }

            const string msDeployArgsFormat = @"-verb:sync -source:package=""{0}"" -dest:auto,computername=""http://{1}:8090/MsDeployAgentService2/"" {3} -allowUntrusted -setParam:""IIS Web Application Name""=""{2}"" -verbose";
            var executableArgs = string.Format(msDeployArgsFormat, sourcePackagePath, targetMachineName,
                                               iisApplicationName, ignore);

            RunProcess(MsWebDeployPath, executableArgs, logger);
           
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
