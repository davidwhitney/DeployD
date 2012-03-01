using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class ConfigTransformationDeploymentHook : DeploymentHookBase
    {
        private ILog _logger = LogManager.GetLogger("ConfigTransformDeploymentHook");

        public ConfigTransformationDeploymentHook(IAgentSettings agentSettings) : base(agentSettings)
        {
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("service");

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
            string searchString = string.Empty;
            //  find base config files
            if (context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website"))
            {
                searchString = "web.config";
            }
            else
            {
                searchString = context.Package.Title + ".exe.config";
            }

            var configFiles = context.Package.GetFiles().Where(f=>Path.GetFileName(f.Path).Equals(searchString, StringComparison.CurrentCultureIgnoreCase));

            foreach(var configFile in configFiles)
            {
                string expectedTransformFileName = Path.GetFileNameWithoutExtension(configFile.Path)
                     + AgentSettings.DeploymentEnvironment + Path.GetExtension(configFile.Path);
                
                string baseConfigurationPath = Path.Combine(context.WorkingFolder, configFile.Path);
                string transformFilePath = Path.Combine(context.WorkingFolder, Path.Combine(configFile.Path, expectedTransformFileName));
                string outputPath = Path.Combine(context.TargetInstallationFolder, configFile.Path);

                if (File.Exists(transformFilePath))
                {
                    // todo: perform transform here
                    _logger.InfoFormat("Transform {0} using {1} to {2}", baseConfigurationPath, transformFilePath, outputPath);
                }
            }
        }
    }
}
