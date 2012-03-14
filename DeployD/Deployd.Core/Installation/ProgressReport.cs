namespace Deployd.Core.Installation
{
    public class ProgressReport
    {
        public string Version { get; private set; }
        public string InstallationTaskId { get; private set; }
        public string PackageId { get; private set; }
        public string Message { get; private set; }

        public ProgressReport(string packageId, string version, string installationTaskId, string message)
        {
            PackageId = packageId;
            Version = version;
            InstallationTaskId = installationTaskId;
            Message = message;
        }

        public static ProgressReport Info(string packageId, string version, string taskId, string message)
        {
            return new ProgressReport(packageId, version, taskId, message);
        }

        public static ProgressReport InfoFormat(string packageId, string version, string taskId, string message, params object[] args)
        {
            return Info(packageId, version, taskId, string.Format(message, args));
        }
    }
}