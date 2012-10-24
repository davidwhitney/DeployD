using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Core.Installation.Hooks
{
    public class ConfigTransformationDeploymentHook : DeploymentHookBase
    {
        public ConfigTransformationDeploymentHook(IFileSystem fileSystem, IAgentSettingsManager agentSettingsManager) : base(agentSettingsManager, fileSystem)
        {
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return true;
        }

        public override void BeforeDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            
        }

        public override void AfterDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            FindAndTransformAppropriateConfiguration(context, reportProgress);
        }

        private void FindAndTransformAppropriateConfiguration(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            var installationLogger = context.GetLoggerFor(this);

            reportProgress(new ProgressReport(context, GetType(), "Transforming configuration files"));

            //  find base config files
            string baseConfigurationFileName = "app.config";
            string expectedTransformFileName = string.Format("app.{0}{1}",
                                                             AgentSettingsManager.Settings.DeploymentEnvironment,
                                                             Path.GetExtension(baseConfigurationFileName));
            string outputFileName = string.Format("{0}.exe.config", context.Package.Title);

            if (context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website"))
            {
                baseConfigurationFileName = "web.config";
                expectedTransformFileName = string.Format("web.{0}.config", AgentSettingsManager.Settings.DeploymentEnvironment);
                outputFileName = "web.config";
            }
            var baseConfigurationPath = Path.Combine(context.WorkingFolder + "\\config", baseConfigurationFileName);
            var transformFilePath = Path.Combine(context.WorkingFolder + "\\config", expectedTransformFileName);
            
            if (!File.Exists(baseConfigurationPath))
            {
                installationLogger.DebugFormat("Config file not found, expected {0}", baseConfigurationPath);
            }

            // find a matching transform
            if (File.Exists(transformFilePath))
            {
                var outputPath = Path.Combine(context.TargetInstallationFolder, outputFileName);
                var transformArgs = string.Format(@"--source=""{0}"" --transform=""{1}"" --destination=""{2}""",
                                                    baseConfigurationPath,
                                                    transformFilePath,
                                                    outputPath);
                RunProcess(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"tools\TransformVsConfiguration.exe"),
                            transformArgs, installationLogger);
            }
        }

        public override string ProgressMessage
        {
            get { return "Configuration Transformations"; }
        }
    }
}
