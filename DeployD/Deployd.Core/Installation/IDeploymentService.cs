using System;
using System.Threading;
using NuGet;

namespace Deployd.Core.Installation
{
    public interface IDeploymentService
    {
        InstallationResult InstallPackage(string packageId, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        InstallationResult InstallPackage(string packageId, string specificVersion, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        bool Deploy(string taskId, IPackage package, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        
    }
}