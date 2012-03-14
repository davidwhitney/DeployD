using System.Collections.Generic;

namespace Deployd.Core.Installation
{
    public interface IInstallationManager
    {
        void StartInstall(string packageId, string version);
        List<InstallationTask> GetAllTasks();
        InstallationTask GetTaskById(string installationTaskId);
    }
}