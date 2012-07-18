using System;

namespace Deployd.Core.Installation
{
    public class ProgressReport
    {
        public string Version { get; private set; }
        public string InstallationTaskId { get; private set; }
        public string PackageId { get; private set; }
        public string Message { get; private set; }
        public string Level { get; private set; }
        
        public DeploymentContext Context { get; private set; }

        public Exception Exception { get; private set; }

        public Type ReportingType { get; set; }

        public ProgressReport(DeploymentContext deploymentContext, Type reportingType, string packageId, string version, string installationTaskId, string message, string level="Info", Exception exception=null)
        {
            Level = level;
            PackageId = packageId;
            Version = version;
            InstallationTaskId = installationTaskId;
            Message = message;
            Exception = exception;
            Context = deploymentContext;
            ReportingType = reportingType;
        }
        public ProgressReport(DeploymentContext deploymentContext, Type reportingType, string message, string level = "Info", Exception exception = null)
        {
            Level = level;
            if (Context != null && Context.Package != null)
            {
                PackageId = Context.Package.Id;
                Version = Context.Package.Version.ToString();
            }
            if (Context != null)
            {
                InstallationTaskId = Context.InstallationTaskId;
            }
            Message = message;
            Exception = exception;
            Context = deploymentContext;
            ReportingType = reportingType;
        }


        public static ProgressReport Info(DeploymentContext deploymentContext, object sender, string packageId, string version, string taskId, string message)
        {
            return new ProgressReport(deploymentContext, sender.GetType(), packageId, version, taskId, message);
        }
        public static ProgressReport Debug(DeploymentContext deploymentContext, object sender, string packageId, string version, string taskId, string message)
        {
            return new ProgressReport(deploymentContext, sender.GetType(), packageId, version, taskId, message, "Debug");
        }
        public static ProgressReport Warn(DeploymentContext deploymentContext, object sender, string packageId, string version, string taskId, string message, Exception exception = null)
        {
            return new ProgressReport(deploymentContext, sender.GetType(), packageId, version, taskId, message, "Warn", exception);
        }
        public static ProgressReport Error(DeploymentContext deploymentContext, object sender, string packageId, string version, string taskId, string message, Exception exception = null)
        {
            return new ProgressReport(deploymentContext, sender.GetType(), packageId, version, taskId, message, "Error", exception);
        }
        public static ProgressReport Fatal(DeploymentContext deploymentContext, object sender, string packageId, string version, string taskId, string message, Exception exception = null)
        {
            return new ProgressReport(deploymentContext, sender.GetType(), packageId, version, taskId, message, "Fatal", exception);
        }

        public static ProgressReport InfoFormat(DeploymentContext deploymentContext, object sender, string packageId, string version, string taskId, string message, params object[] args)
        {
            return Info(deploymentContext, sender.GetType(), packageId, version, taskId, string.Format(message, args));
        }
    }
}