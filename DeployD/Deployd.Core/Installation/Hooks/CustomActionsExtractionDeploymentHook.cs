using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Core.Installation.Hooks
{
    public class CustomActionsExtractionDeploymentHook : IDeploymentHook
    {
        private readonly IFileSystem _fileSystem;

        public CustomActionsExtractionDeploymentHook(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool HookValidForPackage(DeploymentContext context)
        {
            return true;
        }

        public void BeforeDeploy(DeploymentContext context)
        {
        }

        public void Deploy(DeploymentContext context)
        {
        }

        public void AfterDeploy(DeploymentContext context)
        {
            string customActionsFolder = Path.Combine(context.WorkingFolder, "customActions");
            string customActionsInstallFolder =
                Path.Combine(Path.Combine(AgentSettings.AgentProgramDataPath, "customActions"), context.Package.Id);

            if (!_fileSystem.Directory.Exists(customActionsInstallFolder))
                _fileSystem.Directory.CreateDirectory(customActionsInstallFolder);

            if (_fileSystem.Directory.Exists(customActionsFolder))
            {
                var files = _fileSystem.Directory.GetFiles(customActionsFolder, "*.ps1");

                if (!_fileSystem.Directory.Exists(customActionsInstallFolder))
                    _fileSystem.Directory.CreateDirectory(customActionsInstallFolder);
                foreach (var filename in files)
                {
                    var file = _fileSystem.FileInfo.FromFileName(filename);
                    _fileSystem.File.Copy(file.FullName, Path.Combine(customActionsInstallFolder, file.Name), true);
                }
            }
        }
    }
}
