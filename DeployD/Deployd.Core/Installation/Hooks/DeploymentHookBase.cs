using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Core.Installation.Hooks
{
    /// <summary>
    /// Copies files from package to convention dictated destination folder
    /// </summary>
    public abstract class DeploymentHookBase : IDeploymentHook
    {
        protected readonly IAgentSettings AgentSettings;
        private readonly IFileSystem _fileSystem;

        protected DeploymentHookBase(IAgentSettings agentSettings, IFileSystem fileSystem)
        {
            AgentSettings = agentSettings;
            _fileSystem = fileSystem;
        }

        public abstract bool HookValidForPackage(DeploymentContext context);
        public virtual void BeforeDeploy(DeploymentContext context){}
        public virtual void Deploy(DeploymentContext context){}
        public virtual void AfterDeploy(DeploymentContext context) { }

        protected void CopyAllFilesToDestination(DeploymentContext context)
        {
            var installationLogger = context.GetLoggerFor(this);
            // this is where file copy will occur
            var sourceFolder = new DirectoryInfo(Path.Combine(context.WorkingFolder, "content"));

			if (!Directory.Exists(context.TargetInstallationFolder))
			{
				Directory.CreateDirectory(context.TargetInstallationFolder);
			}

            CleanDestinationFolder(context, installationLogger);

            try
            {
                RecursiveCopy(sourceFolder, context.TargetInstallationFolder, installationLogger);
            }
            catch (Exception exception)
            {
                installationLogger.Fatal("Copy failed", exception);
            }
        }

        private void CleanDestinationFolder(DeploymentContext context, ILog logger)
        {
            // wait 1 second for processes to release locks on destination files
            System.Threading.Thread.Sleep(1000);

            new TryThis(() => _fileSystem.Directory.Delete(context.TargetInstallationFolder, true), logger)
                .UpTo(10).Times
                .Go();
        }

        private void RecursiveCopy(DirectoryInfo from, string to, ILog logger)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }

            var subfolders = from.GetDirectories();
            foreach(var subfolder in subfolders)
            {
                var destinationFolder = Path.Combine(to, subfolder.Name);
                RecursiveCopy(subfolder, destinationFolder, logger);
            }

            var files = from.GetFiles().ToArray();
            for (var fileIndex = 0; fileIndex < files.Count(); fileIndex++)
            {
                var destinationFile = Path.Combine(to, files[fileIndex].Name);
                logger.InfoFormat("Copying {0}", destinationFile);

                var index = fileIndex;
                new TryThis(() => files[index].CopyTo(destinationFile, true), logger)
                    .UpTo(10).Times
                    .Go();
            }
        }

        protected bool EnvironmentIsValidForPackage(DeploymentContext context)
        {
            var tags = context.Package.Tags.ToLower().Split(' ', ',', ';');
            return tags.Intersect(AgentSettings.Tags).Any();
        }

        protected void RunProcess(string executablePath, string executableArgs, ILog logger)
        {
            logger.InfoFormat("{0} {1}", executablePath, executableArgs);
            var msDeploy = new Process
                               {
                                   StartInfo =
                                       {
                                           UseShellExecute = false,
                                           RedirectStandardError = true,
                                           RedirectStandardOutput = true,
                                           FileName = executablePath,
                                           Arguments = executableArgs
                                       }
                               };
            msDeploy.Start();

            while (!msDeploy.HasExited)
            {
                var output = msDeploy.StandardOutput.ReadToEnd();
                var error = msDeploy.StandardError.ReadToEnd();

                logger.Info(output);
                if (error.Length > 0)
                {
                    logger.Error(error);
                    throw new ApplicationException("An error occurred running process '"+Path.GetFileName(executablePath)+"'",
                        new Exception(error));
                }

                msDeploy.WaitForExit(2000);
            }
        }
    }
}
