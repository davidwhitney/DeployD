using NuGet;

namespace Deployd.Core.Deployment
{
    public class DeploymentContext
    {
        private readonly IPackage _package;
        private readonly string _workingFolder;
        private readonly string _installationTaskId;

        public DeploymentContext(IPackage package, string workingFolder, string targetInstallationFolder, string installationTaskId)
        {
            _package = package;
            _workingFolder = workingFolder;
            _installationTaskId = installationTaskId;
            TargetInstallationFolder = targetInstallationFolder;
        }

        public string TargetInstallationFolder { get; set; }
        public string WorkingFolder { get { return _workingFolder; } }
        public IPackage Package { get { return _package; } }
        public string InstallationTaskId { get { return _installationTaskId; } }
    }
}