using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public class DeploymentContext
    {
        private readonly IPackage _package;
        private readonly string _workingFolder;
        private string _targetInstallationFolder;

        public DeploymentContext(IPackage package, string workingFolder, string targetInstallationFolder)
        {
            _package = package;
            _workingFolder = workingFolder;
            _targetInstallationFolder = targetInstallationFolder;
        }

        public string TargetInstallationFolder { get { return _targetInstallationFolder; } set { _targetInstallationFolder = value; } }
        public string WorkingFolder { get { return _workingFolder; } }
        public IPackage Package { get { return _package; } }
    }
}