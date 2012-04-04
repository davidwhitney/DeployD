using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Core.Installation.Hooks
{
    public class ConfigTransformationDeploymentHook : DeploymentHookBase
    {
        public ConfigTransformationDeploymentHook(IFileSystem fileSystem, IAgentSettings agentSettings) : base(agentSettings, fileSystem)
        {
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return true;
        }

        public override void AfterDeploy(DeploymentContext context)
        {
            var installationLogger = context.GetLoggerFor(this);
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

                installationLogger.DebugFormat("looking for {0}", transformFilePath);
                
                if (File.Exists(transformFilePath))
                {
                    // todo: perform transform here
                    installationLogger.InfoFormat(@"Transform ""{0}"" using ""{1}"" to ""{2}""", baseConfigurationPath, transformFilePath, outputPath);
                    var transformArgs = string.Format(@"--source=""{0}"" --transform=""{1}"" --destination=""{2}""", 
                        baseConfigurationPath,
                        transformFilePath,
                        outputPath);
                    RunProcess(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"tools\TransformVsConfiguration.exe"), transformArgs, installationLogger);
                } 
                else
                {
                    installationLogger.DebugFormat("No transform found for {0}", baseConfigurationPath);
                }
            }
        }
    }
}
