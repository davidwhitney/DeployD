using Deployd.Core.Deployment;

namespace Deployd.Core.Installation
{
    public class ProgressReport
    {
        public string Version { get; private set; }
        public string InstallationTaskId { get; private set; }
        public string PackageId { get; private set; }
        public string Message { get; private set; }
        public DeploymentContext Context { get; private set; }

        public ProgressReport(DeploymentContext deploymentContext, string packageId, string version, string installationTaskId, string message)
        {
            PackageId = packageId;
            Version = version;
            InstallationTaskId = installationTaskId;
            Message = message;
            Context = deploymentContext;
        }

        

        public static ProgressReport Info(DeploymentContext deploymentContext, string packageId, string version, string taskId, string message)
        {
            return new ProgressReport(deploymentContext, packageId, version, taskId, message);
        }

        public static ProgressReport InfoFormat(DeploymentContext deploymentContext, string packageId, string version, string taskId, string message, params object[] args)
        {
            return Info(deploymentContext, packageId, version, taskId, string.Format(message, args));
        }
    }
}