using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    // copies files to destination folder
    public abstract class DeploymentHookBase : IDeploymentHook
    {
        protected readonly IAgentSettings AgentSettings;
        protected ILog _logger = LogManager.GetLogger("DefaultDeploymentHook");

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

            // clean the destination folder
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
                        _logger.Fatal("Failed to clean destination");
                        throw;
                    }
                    _logger.Warn("Could not clean destination", ex);
                    _logger.WarnFormat("Will retry {0} more times", retryCount);
                    System.Threading.Thread.Sleep(1000);
                }
            }

            try
            {
                RecursiveCopy(sourceFolder, context.TargetInstallationFolder);
            }
            catch (Exception exception)
            {
                _logger.Fatal("Copy failed", exception);
            }
        }

        private void RecursiveCopy(DirectoryInfo from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            var subfolders = from.GetDirectories();
            foreach(var subfolder in subfolders)
            {
                string destinationFolder = Path.Combine(to, subfolder.Name);
                RecursiveCopy(subfolder, destinationFolder);
            }

            var files = from.GetFiles().ToArray();
            for (int fileIndex = 0; fileIndex < files.Count(); fileIndex++)
            {
                string destinationFile = Path.Combine(to, files[fileIndex].Name);
                _logger.InfoFormat("Copying {0}", destinationFile);

                int retryCount = 10;
                try
                {
                    files[fileIndex].CopyTo(destinationFile, true);
                }
                catch (IOException exception)
                {
                    _logger.Warn("Copy failed", exception);
                    if (retryCount > 0)
                    {
                        _logger.WarnFormat("Will retry {0} more times", retryCount);
                        System.Threading.Thread.Sleep(5000);
                        fileIndex--;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public virtual void AfterDeploy(DeploymentContext context){}

        protected bool EnvironmentIsValidForPackage(DeploymentContext context)
        {
            var tags = context.Package.Tags.ToLower().Split(' ', ',', ';');
            if (tags.Intersect(AgentSettings.Tags).Any() 
                /*&& tags.Contains(AgentSettings.DeploymentEnvironment.ToLower())*/) // we're not worried about staging/production only server role
                return true;
            return false;
        }

        protected void RunProcess(string executablePath, string executableArgs)
        {
            _logger.InfoFormat("{0} {1}", executablePath, executableArgs);
            Process msDeploy = new Process();
            msDeploy.StartInfo.UseShellExecute = false;
            msDeploy.StartInfo.RedirectStandardError = true;
            msDeploy.StartInfo.RedirectStandardOutput = true;
            msDeploy.StartInfo.FileName = executablePath;
            msDeploy.StartInfo.Arguments = executableArgs;
            msDeploy.Start();

            while (!msDeploy.HasExited)
            {
                string output = msDeploy.StandardOutput.ReadToEnd();
                string error = msDeploy.StandardError.ReadToEnd();

                _logger.Info(output);
                if (error.Length > 0)
                {
                    _logger.Error(error);
                }

                msDeploy.WaitForExit(2000);
            }
        }
    }
}
