using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class ConfigTransformationDeploymentHook : DeploymentHookBase
    {
        public ConfigTransformationDeploymentHook(IFileSystem fileSystem, IAgentSettings agentSettings) : base(agentSettings, fileSystem)
        {
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return true;
            //return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("service");

            string searchString = string.Empty;
            //  find base config files
            if (context.Package.Tags.ToLower().Split(' ',',',';').Contains("website"))
            {
                searchString = "web.config";
            } else
            {
                searchString = context.Package.Title + ".exe.config";
            }

            return context.Package.GetFiles().Any(f => Path.GetFileName(f.Path).Equals(searchString, StringComparison.CurrentCultureIgnoreCase));
        }

        private void RecursivelyFindFileByName(DirectoryInfo folder, string fileName, ref List<FileInfo> matches )
        {
            if (matches == null)
            {
                matches = new List<FileInfo>();
            }
            matches.AddRange(folder.GetFiles().ToArray().Where(
                    f => f.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)));

            var subFolders = folder.GetDirectories();
            foreach(var subFolder in subFolders)
            {
                RecursivelyFindFileByName(subFolder, fileName, ref matches);
            }
        }

        public override void AfterDeploy(DeploymentContext context)
        {
            //  find base config files
            if (context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website"))
            {
            }

            var configFiles = context.Package.GetFiles().Where(f=>f.Path.ToLower().StartsWith("config\\", StringComparison.InvariantCultureIgnoreCase));

            foreach(var configFile in configFiles)
            {
                var expectedTransformFileName = string.Format("{0}.{1}{2}",
                    Path.GetFileNameWithoutExtension(configFile.Path),
                    AgentSettings.DeploymentEnvironment,
                    Path.GetExtension(configFile.Path));


                var baseConfigurationPath = Path.Combine(context.WorkingFolder, configFile.Path);
                var transformFilePath = Path.Combine(context.WorkingFolder, Path.Combine(Path.GetDirectoryName(configFile.Path), expectedTransformFileName));
                var outputPath = Path.Combine(context.TargetInstallationFolder, configFile.Path);

                Logger.DebugFormat("looking for {0}", transformFilePath);
                
                if (File.Exists(transformFilePath))
                {
                    // todo: perform transform here
                    Logger.InfoFormat(@"Transform ""{0}"" using ""{1}"" to ""{2}""", baseConfigurationPath, transformFilePath, outputPath);
                    var transformArgs = string.Format(@"--source=""{0}"" --transform=""{1}"" --destination=""{2}""", 
                        baseConfigurationPath,
                        transformFilePath,
                        outputPath);
                    RunProcess(Path.Combine(Environment.CurrentDirectory, @"tools\TransformVsConfiguration.exe"), transformArgs);
                } 
                else
                {
                    Logger.DebugFormat("No transform found for {0}", baseConfigurationPath);
                }
            }
        }
    }
}
