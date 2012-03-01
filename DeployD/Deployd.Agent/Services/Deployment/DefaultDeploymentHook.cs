using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace Deployd.Agent.Services.Deployment
{
    // copies files to destination folder
    public class DefaultDeploymentHook : IDeploymentHook
    {
        private ILog _logger = LogManager.GetLogger("DefaultDeploymentHook");
        public virtual bool BeforeDeploy(DeploymentContext context){return false;}

        public virtual bool Deploy(DeploymentContext context)
        {
            // this is where file copy will occur
            string installationFolder = @"d:\wwwcom\" + context.Package.Title;
            var sourceFolder = new DirectoryInfo(Path.Combine(context.WorkingFolder, "content"));

            RecursiveCopy(sourceFolder, installationFolder);

            return true;
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

            var files = from.GetFiles();
            foreach(var file in files)
            {
                string destinationFile = Path.Combine(to, file.Name);
                _logger.DebugFormat("copying {0}", destinationFile);
                file.CopyTo(destinationFile, true);
            }
        }

        public virtual bool AfterDeploy(DeploymentContext context){return false;}
    }
}
