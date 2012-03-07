using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    /// <summary>
    /// Copies files from package to convention dictated destination folder
    /// </summary>
    public abstract class DeploymentHookBase : IDeploymentHook
    {
        protected readonly IAgentSettings AgentSettings;
        protected ILog Logger = LogManager.GetLogger("DefaultDeploymentHook");

        protected DeploymentHookBase(IAgentSettings agentSettings)
        {
            AgentSettings = agentSettings;
        }

        public abstract bool HookValidForPackage(DeploymentContext context);

        public virtual void BeforeDeploy(DeploymentContext context){}

        public virtual void Deploy(DeploymentContext context){}

        protected void CopyAllFilesToDestination(DeploymentContext context)
        {
            // this is where file copy will occur
            var sourceFolder = new DirectoryInfo(Path.Combine(context.WorkingFolder, "content"));

            CleanDestinationFolder(context);

            try
            {
                RecursiveCopy(sourceFolder, context.TargetInstallationFolder);
            }
            catch (Exception exception)
            {
                Logger.Fatal("Copy failed", exception);
            }
        }

        private void CleanDestinationFolder(DeploymentContext context)
        {
            // wait 1 second for processes to release locks on destination files
            System.Threading.Thread.Sleep(1000);

            int retryCount = 10;
            bool cleaned = false;

            while (!cleaned && retryCount-- > 0)
            {
                try
                {
                    Directory.Delete(context.TargetInstallationFolder, true);
                    cleaned = true;
                }
                catch (Exception ex)
                {
                    if (retryCount == 0)
                    {
                        Logger.Fatal("Failed to clean destination");
                        throw;
                    }
                    Logger.Warn("Could not clean destination", ex);
                    Logger.WarnFormat("Will retry {0} more times", retryCount);
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        private void RecursiveCopy(DirectoryInfo from, string to)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }

            var subfolders = from.GetDirectories();
            foreach(var subfolder in subfolders)
            {
                var destinationFolder = Path.Combine(to, subfolder.Name);
                RecursiveCopy(subfolder, destinationFolder);
            }

            var files = from.GetFiles().ToArray();
            for (var fileIndex = 0; fileIndex < files.Count(); fileIndex++)
            {
                var destinationFile = Path.Combine(to, files[fileIndex].Name);
                Logger.InfoFormat("Copying {0}", destinationFile);

                try
                {
                    files[fileIndex].CopyTo(destinationFile, true);
                }
                catch (IOException exception)
                {
                    Logger.Warn("Copy failed", exception);
                    System.Threading.Thread.Sleep(5000);
                    fileIndex--;
                }
            }
        }

        public virtual void AfterDeploy(DeploymentContext context){}

        protected bool EnvironmentIsValidForPackage(DeploymentContext context)
        {
            var tags = context.Package.Tags.ToLower().Split(' ', ',', ';');
            return tags.Intersect(AgentSettings.Tags).Any();
        }

        protected void RunProcess(string executablePath, string executableArgs)
        {
            Logger.InfoFormat("{0} {1}", executablePath, executableArgs);
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

                Logger.Info(output);
                if (error.Length > 0)
                {
                    Logger.Error(error);
                }

                msDeploy.WaitForExit(2000);
            }
        }
    }
}
