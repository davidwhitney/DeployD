using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public class DeploymentContext
    {
        private readonly IPackage _package;
        private readonly string _workingFolder;

        public DeploymentContext(IPackage package, string workingFolder)
        {
            _package = package;
            _workingFolder = workingFolder;
        }

        public string WorkingFolder { get { return _workingFolder; } }
        public IPackage Package { get { return _package; } }
    }
}